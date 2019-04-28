using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Bryllite.Util.Log;

namespace Bryllite.Net.Tcp
{
    public class TcpSession : ITcpSession
    {
        private ILoggable Logger;

        public static readonly int HEADER_SIZE = sizeof(int);
        public static readonly int MAX_PACKET_SIZE = 16 * 1024 * 1024;
        private static readonly int DEFAULT_READ_BUFFER_SIZE = 64 * 1024;

        // Event Handler
        public Action<ITcpSession> OnStart;
        public Action<ITcpSession, int> OnStop;
        public Action<ITcpSession, byte[]> OnMessage;
        public Action<ITcpSession, byte[]> OnWrite;

        // session id
        private static ulong _session_seed = 0;
        private static object _session_seed_lock = new object();
        private ulong _session_id;

        public ulong SID { get { return _session_id; } }

        public ulong NextSID
        {
            get
            {
                lock (_session_seed_lock)
                    return ++_session_seed;
            }
        }

        // session socket
        private Socket _socket;

        // session connected?
        private bool _connected;
        public bool Connected
        {
            get { return _socket != null && _socket.Connected; }
        }

        // read buffer
        private byte[] _read_buffer;

        // received data q
        private List<byte> _read_bytes = new List<byte>();

        // header : packet size
        private byte[] _header = new byte[HEADER_SIZE];

        // session context
        public object Context { get; set; }

        // packet size
        public int BytesToRead
        {
            get
            {
                return BitConverter.ToInt32(_header, 0);
            }
        }

        public string RemoteIP
        {
            get
            {
                return ((IPEndPoint)(_socket.RemoteEndPoint)).Address.ToString();
            }
        }

        public int RemotePort
        {
            get
            {
                return ((IPEndPoint)(_socket.RemoteEndPoint)).Port;
            }
        }

        public string Remote
        {
            get
            {
                return $"{RemoteIP}:{RemotePort}";
            }
        }


        public TcpSession(ILoggable logger, Socket socket, int read_buffer_length = 0)
        {
            Logger = logger;

            if (!socket.Connected)
                throw new ArgumentException("Not connected socket");

            // publish new session id
            _session_id = NextSID;

            // socket
            _socket = socket;

            _connected = true;

            // read buffer
            _read_buffer = new byte[read_buffer_length > 0 ? read_buffer_length : DEFAULT_READ_BUFFER_SIZE];
        }

        public void Start()
        {
            // session start event callback
            OnStart?.Invoke(this);

            // start to read
            ReadHeader();
        }

        public void Stop(int reason = 0)
        {
            // session close event callback
            if (_connected)
            {
                _connected = false;
                OnStop?.Invoke(this, reason);
            }

            // close socket
            if (_socket.Connected)
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
                // writed bytes
                int writeBytes = _socket.EndSend(ar);

                // bytes to write ( include header )
                byte[] bytesToSend = (byte[])ar.AsyncState;

                // is it possible if writing is not completed?
                Debug.Assert(bytesToSend.Length == writeBytes);

                // write completed callback
                OnWrite?.Invoke(this, bytesToSend);
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

        private void OnError(string error)
        {
            Logger.error($"OnError(): remote={RemoteIP}:{RemotePort}, error={error}");
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

                    // 읽기 버퍼 초기화
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
