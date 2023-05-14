using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace helpers.Network.Internal.Tcp
{
    internal class TcpConnection
    {
        internal IPEndPoint EndPoint { get; set; }
        internal Thread ConnectionThread { get; private set; }
        internal TcpClient Client { get; private set; }
        internal NetworkStream Stream { get; private set; }

        internal ushort Id { get; private set; }

        internal TcpConnectionState State { get; private set; }

        internal TcpConnection()
        {
            Client = new TcpClient();
            State = TcpConnectionState.Initialized;
        }

        internal TcpConnection(TcpClient client, ushort id)
        {
            Client = client;
            State = TcpConnectionState.Connected;
            Id = id;
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

                State = TcpConnectionState.Connected;
                ConnectionThread = null;
                Stream = Client.GetStream();
            });

            ConnectionThread.Start();
        }

        public void Disconnect()
        {
            Client.Dispose();
            Client = null;
            Stream = null;

            State = TcpConnectionState.Disconnected;
        }

        public void Send(ArraySegment<byte> data)
        {
            if (Stream != null)
            {
                var payload = new TcpPayload(Id, MessageId.Next, data.Array);
                var bytes = payload.ToBytes();

                Stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
