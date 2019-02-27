using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BrylliteLib.Net
{
    public class TCPServerDelegate
    {
        public delegate void OnStartHandler(string host, int port);
        public delegate void OnStopHandler();

        public delegate void OnAcceptHandler(TCPSession session);
        public delegate void OnCloseHandler(TCPSession session, int reason);
        public delegate void OnMessageHandler(TCPSession session, byte[] data);
        public delegate void OnWriteHandler(TCPSession session, int bytesTransferred);
    }

    public class TCPServer
    {
        public static readonly string TAG = "TCPServer";

        // Start/Stop 콜백
        public TCPServerDelegate.OnStartHandler OnStart;
        public TCPServerDelegate.OnStopHandler OnStop;

        // TCP Server 이벤트 핸들러
        public TCPServerDelegate.OnAcceptHandler OnAccept;
        public TCPServerDelegate.OnCloseHandler OnClose;
        public TCPServerDelegate.OnMessageHandler OnMessage;
        public TCPServerDelegate.OnWriteHandler OnWrite;

        // TCP 세션 리스트
        public Dictionary<ulong, TCPSession> _sessions = new Dictionary<ulong, TCPSession>();

        // 서버 엔드포인트
        public string Host { get; private set; }
        public int Port { get; private set; }

        // TCPServer running?
        private bool _is_running = false;

        // 리슨 소켓
        private Socket ListenSocket;

        // 연결된 세션 갯수
        public int SessionCount
        {
            get
            {
                return _sessions.Count;
            }
        }

        public bool Running => _is_running;

        public TCPServer()
        {
        }

        public bool Start(string host, int port, int acceptThreadCount)
        {
            Host = host;
            Port = port;

            try
            {
                // 소켓 생성
                ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // 바인딩
                ListenSocket.Bind(TCPHelper.ToIPEndPoint(host, port));

                // 리슨 ( backlog * Accept Thread Count )
                ListenSocket.Listen(acceptThreadCount * 20);

                // 통신 수락 개시
                BeginAccept(acceptThreadCount);

                // 스타트 콜백
                OnStart?.Invoke(host, port);

                _is_running = true;
                return true;
            }
            catch (Exception e)
            {
                Log.e(TAG, $"Start() Exception! e.Message={e.Message}");
                return false;
            }
        }

        public void Stop()
        {
            if (ListenSocket.IsBound)
            {
                ListenSocket.Close();

                lock (_sessions)
                {
                    // 모든 세션 종료
                    TCPSession[] sessions = _sessions.Values.ToArray();
                    foreach (TCPSession s in sessions)
                    {
                        s.Stop();
                    }

                    _sessions.Clear();
                }

                // 종료 콜백
                OnStop?.Invoke();
            }

            _is_running = false;
        }

        public void SendTo(TCPSession session, byte[] data)
        {
            session.Write(data);
        }

        public void SendAll(byte[] data)
        {
            lock (_sessions)
            {
                TCPSession[] sessions = _sessions.Values.ToArray();
                foreach (TCPSession s in sessions)
                {
                    s.Write(data);
                }
            }
        }

        internal void BeginAccept(int acceptThreadCount = 1)
        {
            try
            {
                for (int i = 0; i < acceptThreadCount; i++)
                    ListenSocket.BeginAccept(OnHandleAccept, ListenSocket);
            }
            catch (Exception e)
            {
                Log.w(TAG, $"BeginAccept(): Exception! e.Message={e.Message}");
            }
        }

        internal void OnHandleAccept(IAsyncResult ar)
        {
            try
            {
                // 연결된 소켓
                Socket acceptSocket = ListenSocket.EndAccept(ar);

                OnAcceptSocket(acceptSocket);

                // 다시 연결 대기
                BeginAccept();
            }
            catch (ObjectDisposedException)
            {
                // 리슨 소켓이 제거되었다.
                Log.d(TAG, "ListenSocket.BeginAccept() canceled");
            }
            catch (SocketException e)
            {
                Log.w(TAG, $"OnHandleAccept(): SocketException! e.Message={e.Message}, e.SocketErrorCode={e.SocketErrorCode}");
            }
            catch (Exception e)
            {
                Log.w(TAG, $"OnHandleAccept(): Exception! e.Message={e.Message}");
            }
        }

        internal void OnAcceptSocket(Socket socket)
        {
            // 세션 생성 및 등록
            TCPSession session = new TCPSession(socket);

            // 세션 콜백 등록
            session.OnStart = OnSessionStart;
            session.OnMessage = OnSessionMessage;
            session.OnStop = OnSessionClose;
            session.OnWrite = OnSessionWrite;

            // 세션 시작
            session.Start();

            lock (_sessions)
            {
                _sessions.Add(session.ID, session);
            }
        }

        internal void OnSessionStart(TCPSession session)
        {
            // 세션 생성 이벤트 콜백
            OnAccept?.Invoke(session);
        }

        internal void OnSessionClose(TCPSession session, int reason)
        {
            ulong session_id = session.ID;

            // 세션 종료 콜백
            OnClose?.Invoke(session, reason);

            lock (_sessions)
            {
                _sessions.Remove(session_id);
            }
        }

        internal void OnSessionMessage(TCPSession session, byte[] data)
        {
            lock (session)
            {
                // 세션 메세지 콜백
                OnMessage?.Invoke(session, data);
            }
        }

        internal void OnSessionWrite(TCPSession session, int bytesTransferred)
        {
            OnWrite?.Invoke(session, bytesTransferred);
        }

        // 세션 id 로 세션을 찾는다.
        public TCPSession FindSession(ulong session_id)
        {
            lock (_sessions)
            {
                if (_sessions.ContainsKey(session_id))
                    return _sessions[session_id];
            }

            return null;
        }

        public TCPSession[] Sessions
        {
            get
            {
                lock (_sessions)
                {
                    return _sessions.Values.ToArray();
                }
            }
        }

        public static string GetLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            return TCPHelper.IP_ANY;
        }
    }
}
