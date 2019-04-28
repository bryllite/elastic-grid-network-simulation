using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Bryllite.Util.Log;

namespace Bryllite.Net.Tcp
{
    public class TcpServer : ITcpServer
    {
        private ILoggable Logger;

        // Event Handler
        public Action<string, int> OnStart;
        public Action OnStop;
        public Action<ITcpSession> OnAccept;
        public Action<ITcpSession, int> OnClose;
        public Action<ITcpSession, byte[]> OnMessage;
        public Action<ITcpSession, byte[]> OnWrite;

        // TCP Session lists
        public Dictionary<ulong, ITcpSession> _sessions = new Dictionary<ulong, ITcpSession>();

        // server local endpoint
        public string Host { get; private set; }
        public int Port { get; private set; }

        // Listen socket
        private Socket AcceptSocket;

        // connected session counts
        public int SessionCount
        {
            get { return _sessions.Count; }
        }

        // is server running?
        private bool _running = false;
        public bool Running => _running;
            

        public TcpServer()
        {
        }

        public TcpServer(ILoggable logger)
        {
            Logger = logger;
        }

        public bool Start(string host, int port, int acceptThreadCount, int backlogs = 32 )
        {
            Host = host;
            Port = port;

            try
            {
                // socket 
                AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                AcceptSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                AcceptSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, true);

                // bind socket
                AcceptSocket.Bind(TcpHelper.ToIPEndPoint(host, port));

                // listen
                AcceptSocket.Listen(acceptThreadCount * backlogs);

                // start accept async
                BeginAccept(acceptThreadCount);

                // start event callback
                OnStart?.Invoke(host, port);

                _running = true;
                return true;
            }
            catch (Exception e)
            {
                Logger.error($"exception! e={e.Message}");
                return false;
            }
        }

        public void Stop()
        {
            if (_running)
            {
                _running = false;

                // close accept socket
                if (AcceptSocket.IsBound)
                    AcceptSocket.Close();

                // close all sessions
                foreach (var s in ToSessionArray())
                    s.Stop();

                _sessions.Clear();

                // stop callback
                OnStop?.Invoke();
            }
        }

        public void SendTo(ITcpSession session, byte[] data)
        {
            session.Write(data);
        }

        public ITcpSession[] ToSessionArray()
        {
            lock (_sessions)
            {
                return _sessions.Values.ToArray();
            }
        }

        public void SendAll(byte[] data)
        {
            foreach (var s in ToSessionArray())
                s.Write(data);
        }

        private void BeginAccept(int acceptThreadCount = 1)
        {
            try
            {
                for (int i = 0; i < acceptThreadCount; i++)
                    AcceptSocket.BeginAccept(OnHandleAccept, i);
            }
            catch (Exception e)
            {
                Logger.error($"exception! e={e.Message}");
            }
        }

        private void OnHandleAccept(IAsyncResult ar)
        {
            try
            {
                // accepted socket
                Socket socket = AcceptSocket.EndAccept(ar);

                // process accepted socket
                OnAcceptSocket(socket);

                // begin accept again
                BeginAccept();
            }
            catch (ObjectDisposedException e)
            {
                Logger.debug($"AcceptSocket.AcceptAsync() canceled! e={e.Message}");
            }
            catch (SocketException e)
            {
                Logger.warning($"SocketException! e={e.Message}");
            }
            catch (Exception e)
            {
                Logger.error($"Exception! e={e.Message}");
            }
        }

        private void OnAcceptSocket(Socket socket)
        {
            // new session
            ITcpSession session = new TcpSession(Logger, socket)
            {
                OnStart = OnSessionStart,
                OnStop = OnSessionStop,
                OnMessage = OnSessionMessage,
                OnWrite = OnSessionWrite
            };

            lock (_sessions)
            {
                _sessions[session.SID] = session;
            }

            // start session
            session.Start();
        }

        private void OnSessionStart(ITcpSession session)
        {
            try
            {
                // session start callback
                OnAccept?.Invoke(session);
            }
            catch (Exception e)
            {
                Logger.error($"OnAccept() exception! e={e.Message}");
                session.Stop();
            }
        }

        private void OnSessionStop(ITcpSession session, int reason)
        {
            ulong sid = session.SID;

            try
            {
                // session stop callback
                OnClose?.Invoke(session, reason);
            }
            catch (Exception e)
            {
                Logger.error($"OnClose() exception! e={e.Message}");
            }

            lock (_sessions)
            {
                _sessions.Remove(sid);
            }

            Logger.debug($"session.count={_sessions.Count}");
        }

        private void OnSessionMessage(ITcpSession session, byte[] data)
        {
            try
            {
                OnMessage?.Invoke(session, data);
            }
            catch (Exception e)
            {
                Logger.error($"Exception! e={e.Message}");
                session.Stop(-1);
            }
        }

        private void OnSessionWrite(ITcpSession session, byte[] data)
        {
            try
            {
                // session write callback
                OnWrite?.Invoke(session, data);
            }
            catch (Exception e)
            {
                Logger.error($"OnWrite() exception! e={e.Message}");
                session.Stop();
            }
        }

        public ITcpSession FindSession(ulong sid)
        {
            lock (_sessions)
            {
                return _sessions.ContainsKey(sid) ? _sessions[sid] : null;
            }
        }
    }
}
