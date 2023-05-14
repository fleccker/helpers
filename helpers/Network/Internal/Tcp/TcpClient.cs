using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace helpers.Network.Internal.Tcp
{
    internal class TcpClient
    {
        internal IPEndPoint EndPoint { get; set; }
        internal Thread ConnectionThread { get; private set; }
        internal System.Net.Sockets.TcpClient Client { get; }

        internal TcpConnectionState State { get; private set; }

        internal TcpClient()
        {
            Client = new System.Net.Sockets.TcpClient();
            State = TcpConnectionState.Initialized;
        }

        public void Connect(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public void Connect(IPAddress address, int port)
        {
            EndPoint = new IPEndPoint(address, port);
        }

        public void Connect(string address, int port)
        {
            if (address is "localhost" || address is "0.0.0.0" || address is "127.0.0.1")
                Connect(IPAddress.Loopback, port);
            else
                Connect(IPAddress.Parse(address), port);
        }

        public void Connect()
        {
            State = TcpConnectionState.Connecting;

            ConnectionThread = new Thread(() =>
            {
                Client.Connect(EndPoint);
            });

            ConnectionThread.Start();
        }
    }
}
