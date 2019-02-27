using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace BrylliteLib.Net
{
    // 이벤트 핸들러
    public class CTcpSessionDelegate
    {
        public delegate void OnStartHandler(TCPSession session);
        public delegate void OnStopHandler(TCPSession session, int reason);
        public delegate void OnMessageHandler(TCPSession session, byte[] data);
        public delegate void OnWriteHandler(TCPSession session, int bytesTransferred);
    }

    // 세션 클래스
    public class TCPSession
    {
        public static readonly string TAG = "TCPSession";

        public static readonly int HEADER_SIZE = sizeof(int);
        public static readonly int MAX_PACKET_SIZE = 16 * 1024 * 1024;
        private static readonly int DEFAULT_READ_BUFFER_SIZE = 64 * 1024;

        // 이벤트 핸들러
        public CTcpSessionDelegate.OnStartHandler OnStart;
        public CTcpSessionDelegate.OnStopHandler OnStop;
        public CTcpSessionDelegate.OnMessageHandler OnMessage;
        public CTcpSessionDelegate.OnWriteHandler OnWrite;

        // 세션 아이디
        private ulong _session_id;
        private static ulong _session_seed = 0;
        private static object _session_seed_lock = new object();

        // 세션 소켓
        private Socket _socket;

        // 세션 연결 상태
        private bool _connected;

        // 읽기 버퍼
        private byte[] _read_buffer;

        // 헤더: 패킷 크기
        private byte[] _header = new byte[HEADER_SIZE];

        // 현재까지 읽은 데이터 모아둔거 ( 실질적인 읽기 버퍼 )
        private List<byte> _read_bytes = new List<byte>();

        // Session Context
        public object Context { get; set; }

        public int BytesToRead
        {
            get
            {
                return BitConverter.ToInt32(_header, 0);
            }
        }

        public TCPSession(Socket socket, int readBufferSize = 0)
        {
            if (!socket.Connected)
                throw new ArgumentException("Not connected socket");

            // 세션 아이디
            lock (_session_seed_lock)
            {
                ID = ++_session_seed % ulong.MaxValue;
            }

            // 소켓
            _socket = socket;

            _connected = true;

            // 읽기 버퍼 생성
            _read_buffer = new byte[readBufferSize > 0 ? readBufferSize : DEFAULT_READ_BUFFER_SIZE];
        }

        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        public ulong ID
        {
            get { return _session_id; }
            private set { _session_id = value; }
        }

        // 세션 시작
        public void Start()
        {
            // 세션 시작 이벤트 콜백
            OnStart?.Invoke(this);

            // 읽기 시작한다
            ReadHeader();
        }

        public void Stop(int reason = 0)
        {
            // 종료 이벤트 핸들러 호출
            if (_connected)
            {
                _connected = false;
                OnStop?.Invoke(this, reason);
            }

            // 소켓 닫기
            if (Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        public int Write(byte[] data)
        {
            if (!Connected)
                return 0;

            if (data.Length <= 0 || data.Length >= MAX_PACKET_SIZE)
                throw new ArgumentException($"Exceeds max packet size ({MAX_PACKET_SIZE})");

            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);
            byte[] bytesToSend = bytes.ToArray();

            try
            {
                _socket.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, OnHandleWrite, bytesToSend);
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                {
                    OnError(e.Message);
                    return -1;
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
                return -1;
            }

            return bytesToSend.Length;
        }

        private void OnHandleWrite(IAsyncResult ar)
        {
            if (!Connected) return;

            try
            {
                // 전송한 바이트
                int writeBytes = _socket.EndSend(ar);

                // 쓰기 데이터( 헤더 포함 )
                byte[] bytesToSend = (byte[])ar.AsyncState;

                // 덜 쓰고 호출되는 경우도 있는가???
                Debug.Assert(bytesToSend.Length == writeBytes);

                // 쓰기 완료 이벤트 핸들러
                OnWrite?.Invoke(this, writeBytes);
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                    OnError(e.Message);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        // 읽기 수행
        public void OnError(string error)
        {
            Log.w(TAG, $"OnError(): error={error}");

            Stop(-1);
        }

        private void ReadHeader()
        {
            if (!Connected) return;

            try
            {
                _socket.BeginReceive(_header, 0, HEADER_SIZE, SocketFlags.None, OnHandleReadHeader, _header);
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                    OnError(e.Message);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnHandleReadHeader(IAsyncResult ar)
        {
            if (!Connected) return;

            int readBytes = 0;
            try
            {
                readBytes = _socket.EndReceive(ar);
                if (readBytes <= 0)
                {
                    Stop(0);
                    return;
                }

                // 항상 헤더 크기만큼 읽는다.
                // 헤더 사이즈 보다 작은 크기가 읽어지는 경우가 있나?
                Debug.Assert(readBytes == HEADER_SIZE);

                // 메세지 크기
                int size = BytesToRead;
                Debug.Assert(size > 0 && size < MAX_PACKET_SIZE);

                // 바디 읽기
                ReadData();
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                {
                    if (readBytes <= 0)
                    {
                        Stop(0);
                        return;
                    }

                    OnError(e.Message);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void ReadData()
        {
            if (!Connected) return;

            int bytesToRead = BitConverter.ToInt32(_header, 0);
            int readSize = Math.Min(bytesToRead - _read_bytes.Count, _read_buffer.Length);

            try
            {
                _socket.BeginReceive(_read_buffer, 0, readSize, SocketFlags.None, OnHandleReadData, this);
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                    OnError(e.Message);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void OnHandleReadData(IAsyncResult ar)
        {
            if (!Connected) return;

            int readBytes = 0;
            try
            {
                readBytes = _socket.EndReceive(ar);
                if (readBytes <= 0)
                {
                    Stop(0);
                    return;
                }

                // 읽기 버퍼에 기록
                _read_bytes.AddRange(_read_buffer.Take(readBytes).ToArray());

                // 만약 다 읽었으면
                if (_read_bytes.Count == BytesToRead)
                {
                    // 수신 콜백
                    OnMessage?.Invoke(this, _read_bytes.ToArray());

                    _read_bytes.Clear();

                    // 헤더 읽기
                    ReadHeader();
                }
                else
                {
                    // 아니면 나머지 데이터 더 읽기
                    ReadData();
                }
            }
            catch (SocketException e)
            {
                if (IsSocketError(e.SocketErrorCode))
                {
                    if (readBytes <= 0)
                    {
                        Stop(0);
                        return;
                    }

                    OnError(e.Message);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }

        }

        private bool IsSocketError(SocketError socketErrorCode)
        {
            switch (socketErrorCode)
            {
                case SocketError.Success:
                case SocketError.IOPending:
                case SocketError.WouldBlock:
                    return false;

                default: break;
            }

            return true;
        }
    }
}
