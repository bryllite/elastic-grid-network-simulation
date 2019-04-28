using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bryllite.Util.Log;

namespace Bryllite.Net.Tcp
{
    public class TcpClient : ITcpClient
    {
        private ILoggable Logger;

        // Event Handler
        public Action<ITcpClient> OnConnect;
        public Action<ITcpClient, string> OnConnectFail;
        public Action<ITcpClient, int> OnDisconnect;
        public Action<ITcpClient, byte[]> OnMessage;
        public Action<ITcpClient, byte[]> OnWrite;

        public enum ConnectState
        {
            Disconnected,
            Connecting,
            Connected
        }

        private ConnectState _connectState = ConnectState.Disconnected;
        public ConnectState State => _connectState;

        // socket
        private Socket _socket;

        // server end point
        public string Host { get; private set; }
        public int Port { get; private set; }

        // session buffer
        private ITcpSession _session;
        private int _session_read_buffer_size;
        public ITcpSession Session => _session;

        // connected?
        public bool Connected => State == ConnectState.Connected ;

        public bool Ready
        {
            get { return Connected && _session != null && _session.Connected; }
        }

        public TcpClient()
        {
        }

        public TcpClient(ILoggable logger)
        {
            Logger = logger;
        }

        public void Start(string host, int port, int session_read_buffer_length = 0)
        {
            // session read buffer length
            _session_read_buffer_size = session_read_buffer_length;

            // connect to host
            Connect(host, port);
        }

        public void Stop()
        {
            if (Connected)
            {
                _connectState = ConnectState.Disconnected;
                _session.Stop();
            }
        }

        public int Send(byte[] data)
        {
            return Ready ? _session.Write(data) : -1;
        }

        private void Connect(string host, int port)
        {
            if (_connectState != ConnectState.Disconnected) return;

            // server end point
            Host = host;
            Port = port;

            // socket
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // connect
                _connectState = ConnectState.Connecting;
                _socket.BeginConnect(TcpHelper.ToIPEndPoint(host, port), OnHandleConnect, _socket);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.IOPending || e.SocketErrorCode != SocketError.WouldBlock)
                    OnConnectFailed(e.Message);
            }
            catch (Exception e)
            {
                OnConnectFailed(e.Message);
            }
        }

        private void OnConnected()
        {
            _connectState = ConnectState.Connected;
            OnConnect?.Invoke(this);
        }

        private void OnConnectFailed(string err)
        {
            _connectState = ConnectState.Disconnected;
            OnConnectFail?.Invoke(this, err);
        }

        private void OnHandleConnect(IAsyncResult ar)
        {
            bool connected = false;

            try
            {
                _socket.EndConnect(ar);
                connected = _socket.Connected;
            }
            catch (Exception e)
            {
                OnConnectFailed(e.Message);
                return;
            }

            if (connected)
            {
                // new session for connected socket
                _session = new TcpSession(Logger, _socket, _session_read_buffer_size)
                {
                    OnStart = OnSessionStart,
                    OnStop = OnSessionClose,
                    OnMessage = OnSessionMessage,
                    OnWrite = OnSessionWrite
                };

                // start session
                _session.Start();
            }
            else
            {
                OnConnectFailed("Not connected");
            }
        }

        private void OnSessionStart(ITcpSession session)
        {
            try
            {
                OnConnected();
            }
            catch (Exception e)
            {
                Logger.error($"OnConnect() exception! e={e.Message}");
                session.Stop();
            }
        }

        private void OnSessionClose(ITcpSession session, int reason)
        {
            try
            {
                _connectState = ConnectState.Disconnected;
                OnDisconnect?.Invoke(this, reason);
            }
            catch (Exception e)
            {
                Logger.error($"OnClose() exception! e={e.Message}");
            }
        }

        private void OnSessionMessage(ITcpSession session, byte[] data)
        {
            try
            {
                OnMessage?.Invoke(this, data);
            }
            catch (Exception e)
            {
                Logger.error($"OnMessage() exception! e={e.Message}");
                session.Stop();
            }
        }

        private void OnSessionWrite(ITcpSession session, byte[] data)
        {
            try
            {
                OnWrite?.Invoke(this, data);
            }
            catch (Exception e)
            {
                Logger.error($"OnWrite() exception! e={e.Message}");
                session.Stop();
            }
        }

        public static async Task<bool> SendToAsync(string host, int port, byte[] data)
        {
            return await Task.Factory.StartNew(() =>
            {
                return SendTo(host, port, data);
            });
        }

        public static bool SendTo(string host, int port, byte[] data)
        {
            return SendTo(host, port, data, null);
        }

        // Send packet to host
        // connect -> send -> close
        public static bool SendTo(string host, int port, byte[] data, ILoggable Logger)
        {
            Debug.Assert(!string.IsNullOrEmpty(host) && data.Length > 0);

            // 패킷 최대 크기
            Debug.Assert(data.Length < TcpSession.MAX_PACKET_SIZE - TcpSession.HEADER_SIZE);
            if (data.Length > TcpSession.MAX_PACKET_SIZE - TcpSession.HEADER_SIZE)
            {
                Logger.warning($"exceeds MAX_PACKET_SIZE({TcpSession.MAX_PACKET_SIZE})");
                return false;
            }

            System.Net.Sockets.TcpClient tcp_client = null;
            try
            {
                tcp_client = new System.Net.Sockets.TcpClient(host, port);
            }
            catch (Exception e)
            {
                Logger.error($"can't connect to Peer! remote={host}:{port}, e={e.Message}");
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
                Logger.error($"can't write to peer! remote={host}:{port}, data.Length={data.Length}, e={e.Message}");
            }
            finally
            {
                // 소켓 종료
                tcp_client.Close();
            }

            return ret;
        }

    }
}
