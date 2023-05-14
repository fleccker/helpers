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
        internal Dictionary<ushort, TcpConnection> _currentClients = new Dictionary<ushort, TcpConnection>();

        internal TcpListener Listener { get; private set; }
        internal IPEndPoint EndPoint { get; set; }

        internal Thread ListenerThread { get; private set; }

        internal TcpServerState State { get; private set; }


        internal EventProvider OnTcpAccepted;

        internal TcpServer(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            State = TcpServerState.Initialized;

            OnTcpAccepted = new EventProvider();
        }

        internal void Start()
        {
            if (State is TcpServerState.Listening) throw new InvalidOperationException($"The TCP server is already running!");

            State = TcpServerState.Listening;

            Listener = new TcpListener(EndPoint);
            Listener.Start();

            ListenerThread = new Thread(Listen);
            ListenerThread.Start();
        }

        internal void Stop()
        {
            if (!(State is TcpServerState.Listening)) throw new InvalidOperationException($"The TCP server is not running!");

            State = TcpServerState.Initialized;

            Listener.Stop();
            Listener = null;
            ListenerThread = null;
        }

        internal void Listen()
        {
            while (State is TcpServerState.Listening)
            {
                var incomingClient = Listener.AcceptTcpClient();
                if (incomingClient == null) continue;

                var peerId = PeerId.Next;

                _currentClients[peerId] = new TcpConnection(incomingClient, peerId);

                OnTcpAccepted.Invoke(new KeyValuePair<string, object>("client", incomingClient),
                                     new KeyValuePair<string, object>("conn", _currentClients[peerId]));


            }
        }
    }
}