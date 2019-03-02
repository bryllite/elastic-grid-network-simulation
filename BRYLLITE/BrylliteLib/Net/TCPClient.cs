using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BrylliteLib.Net
{
    public class TCPClientDelegate
    {
        public delegate void OnConnectHandler(TCPClient tcpClient, bool connected, string error);
        public delegate void OnCloseHandler(TCPClient tcpClient, int reason);
        public delegate void OnMessageHandler(TCPClient tcpClient, byte[] data);
        public delegate void OnWriteHandler(TCPClient tcpClient, int writeBytes);
    }

    public class TCPClient
    {
        public static readonly string TAG = "TCPClient";

        // 이벤트 핸들러
        public TCPClientDelegate.OnConnectHandler OnConnect;
        public TCPClientDelegate.OnCloseHandler OnClose;
        public TCPClientDelegate.OnMessageHandler OnMessage;
        public TCPClientDelegate.OnWriteHandler OnWrite;

        // 소켓
        private Socket _socket;

        // 서버 엔드 포인트
        private string _host;
        private int _port;

        // 세션 버퍼
        private TCPSession _session;
        private int _session_read_buffer_size;

        // is connected?
        private bool _connected = false;
        public bool Connected
        {
            get
            {
                return _connected;
            }
            private set
            {
                _connected = value;
            }
        }

        public bool Ready
        {
            get
            {
                return Connected && _session != null && _session.Connected;
            }
        }

        public TCPClient()
        {
        }

        public void Start(string host, int port, int sessionReadBufferSize = 0)
        {
            // 서버 엔드포인트
            _host = host;
            _port = port;

            // 세션 버퍼 크기
            _session_read_buffer_size = sessionReadBufferSize;

            // 서버에 연결
            Connect(host, port);
        }

        public void Stop()
        {
            if (Connected)
            {
                _session.Stop();
                Connected = false;
            }
        }

        public int Send(byte[] data)
        {
            return _session.Write(data);
        }

        private void Connect(string host, int port)
        {
            // 소켓 생성
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 연결 시작
                _socket.BeginConnect(TCPHelper.ToIPEndPoint(host, port), OnHandleConnect, _socket);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.IOPending || e.SocketErrorCode != SocketError.WouldBlock)
                {
                    OnConnect?.Invoke(this, _socket.Connected, e.Message);
                }
            }
            catch (Exception e)
            {
                OnConnect?.Invoke(this, _socket.Connected, e.Message);
            }
        }

        private void OnHandleConnect(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                Connected = _socket.Connected;
            }
            catch (Exception e)
            {
                //                Log.d(TAG, $"OnHandleConnect(): Exception! e.Message={e.Message}");
                OnConnect?.Invoke(this, false, e.Message);
                return;
            }

            if (Connected)
            {
                // 세션 생성
                _session = new TCPSession(_socket, _session_read_buffer_size);

                // 세션 핸들러 등록
                _session.OnStart = OnSessionStart;
                _session.OnStop = OnSessionClose;
                _session.OnMessage = OnSessionMessage;
                _session.OnWrite = OnSessionWrite;

                // 세션 시작
                _session.Start();
            }
            else
            {
                OnConnect?.Invoke(this, false, "Connection failed");
            }
        }

        private void OnSessionStart(TCPSession session)
        {
            OnConnect?.Invoke(this, Connected, "success");
        }

        private void OnSessionClose(TCPSession session, int reason)
        {
            OnClose?.Invoke(this, reason);
        }

        private void OnSessionMessage(TCPSession session, byte[] data)
        {
            OnMessage?.Invoke(this, data);
        }

        private void OnSessionWrite(TCPSession session, int writeBytes)
        {
            OnWrite?.Invoke(this, writeBytes);
        }

        public static async Task<bool> SendToAsync( string host, int port, byte[] data )
        {
            return await Task.Factory.StartNew(() =>
            {
                return SendTo(host, port, data);
            });
        }

        // Send packet to host
        // connect -> send -> close
        public static bool SendTo(string host, int port, byte[] data)
        {
            Debug.Assert(!string.IsNullOrEmpty(host) && data.Length > 0);

            // 패킷 최대 크기
            Debug.Assert(data.Length < TCPSession.MAX_PACKET_SIZE - TCPSession.HEADER_SIZE);
            if (data.Length > TCPSession.MAX_PACKET_SIZE - TCPSession.HEADER_SIZE)
            {
                Log.w(TAG, $"SendTo(): exceeds MAX_PACKET_SIZE({TCPSession.MAX_PACKET_SIZE})");
                return false;
            }

            TcpClient tcp_client = null ;
            try
            {
                tcp_client = new TcpClient(host, port);

                // set client socket reuse address
//                tcp_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // connect
//                tcp_client.Connect(host, port);

            }
            catch (Exception e)
            {
                Log.d(TAG, $"SendTo(): can't connect to Peer! remote={host}:{port}, e={e.Message}");
                return false;
            }

            // 전송할 데이터
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);
            byte[] bytesToSend = bytes.ToArray();

            bool ret = false;
            try
            {
                // write data to socket
                NetworkStream ns = tcp_client.GetStream();
                ns.Write(bytesToSend, 0, bytesToSend.Length);

                ret = true;
            }
            catch (Exception e)
            {
                Log.d(TAG, $"SendTo(): can't write to peer! remote={host}:{port}, data={data.SimpleHexString()}, data.Length={data.Length}, e={e.Message}");
            }
            finally
            {
                // 소켓 종료
                tcp_client.Close();
            }

            return ret;
        }

        public static byte[] SendToAndReadAck(string host, int port, byte[] data)
        {
            // 패킷 최대 크기
            Debug.Assert(data.Length < TCPSession.MAX_PACKET_SIZE - TCPSession.HEADER_SIZE);
            if (data.Length > TCPSession.MAX_PACKET_SIZE - TCPSession.HEADER_SIZE)
            {
                Log.w(TAG, $"SendToAndReadAck(): exceeds MAX_PACKET_SIZE({TCPSession.MAX_PACKET_SIZE})");
                return null;
            }

            // TCP client
            TcpClient tcpClient = new TcpClient();

            try
            {

                // connect
                tcpClient.Connect(host, port);
            }
            catch (Exception e)
            {
                Log.d(TAG, $"SendToAndReadAck(): can't connect to Peer! remote={host}:{port}, e={e.Message}");
                return null;
            }

            // 전송할 데이터
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);
            byte[] bytesToSend = bytes.ToArray();

            NetworkStream ns;

            try
            {
                // write data to socket
                ns = tcpClient.GetStream();
                ns.Write(bytesToSend, 0, bytesToSend.Length);
            }
            catch (Exception e)
            {
                Log.d(TAG, $"SendToAndReadAck(): can't write to peer! remote={host}:{port}, data={data.SimpleHexString()}, data.Length={data.Length}, e={e.Message}");
                return null;
            }

            // 응답 데이터
            List<byte> bytesRead = new List<byte>();

            try
            {
                // receive ack from peer
                byte[] bytesToRead = new byte[TCPSession.HEADER_SIZE];

                // read header
                int reads = ns.Read(bytesToRead, 0, TCPSession.HEADER_SIZE);
                Debug.Assert(reads == TCPSession.HEADER_SIZE);
                int header = BitConverter.ToInt32(bytesToRead, 0);
                Debug.Assert(header > 0 && header < TCPSession.MAX_PACKET_SIZE);

                // 데이터 읽는다. 8K씩 잘라 읽는다.
                byte[] buffer = new byte[8192];
                while (bytesRead.Count < header)
                {
                    reads = ns.Read(buffer, 0, buffer.Length);
                    if (reads <= 0)
                        break;

                    bytesRead.AddRange(buffer.Take(reads).ToArray());
                }

                // 소켓 종료
                tcpClient.Close();

                // 읽은 데이터 리턴
                return bytesRead.ToArray();
            }
            catch (Exception e)
            {
                Log.w(TAG, $"SendToAndReadAck(): can't read ack from peer! remote={host}:{port}, e={e.Message}");
                return null;
            }
        }

        public static async Task<byte[]> SendToAndReadAckAsync(string host, int port, byte[] data)
        {
            return await Task.Factory.StartNew(() =>
            {
                return SendToAndReadAck(host, port, data);
            });
        }

    }
}
