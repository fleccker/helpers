using System.Net;
using System.Net.Sockets;

using System;
using System.Threading;
using System.Collections.Generic;

using helpers.Events;

namespace helpers.Network.Internal.Tcp
{
    internal class TcpServer
    {
        internal TcpListener Listener { get; private set; }
        internal IPEndPoint EndPoint { get; set; }

        internal Thread ListenerThread { get; private set; }

        internal bool IsRunning { get; private set; }

        internal EventProvider OnTcpAccepted;

        internal TcpServer(IPEndPoint endPoint)
        {
            EndPoint = endPoint;

            OnTcpAccepted = new EventProvider();
        }

        internal void Start()
        {
            if (IsRunning) throw new InvalidOperationException($"The TCP server is already running!");

            IsRunning = true;

            Listener = new TcpListener(EndPoint);
            Listener.Start();

            ListenerThread = new Thread(Listen);
            ListenerThread.Start();
        }

        internal void Stop()
        {
            if (!IsRunning) throw new InvalidOperationException($"The TCP server is not running!");

            IsRunning = false;

            Listener.Stop();
            Listener = null;
            ListenerThread = null;
        }

        internal void Listen()
        {
            while (IsRunning)
            {
                var incomingClient = Listener.AcceptTcpClient();
                if (incomingClient == null) continue;
                
                OnTcpAccepted.Invoke(new KeyValuePair<string, object>("client", incomingClient));
            }
        }
    }
}