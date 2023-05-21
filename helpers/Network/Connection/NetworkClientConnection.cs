using helpers.Events;
using helpers.Network.Internal;
using helpers.Network.Internal.Tcp;
using helpers.Network.Transport;
using helpers.Network.Transport.Packets.Internal;

using System;
using System.Net;
using System.Collections.Generic;

namespace helpers.Network.Connection
{
    public class NetworkClientConnection : NetworkConnection
    {
        internal TcpConnection Connection { get; } 

        public IPEndPoint EndPoint { get => Connection.EndPoint; }

        public bool IsConnected { get => Connection.ActualState is TcpConnectionState.Connected; }
        public bool IsReady { get => Connection.State != TcpConnectionState.NotInitialized; }
        public bool IsSynchronized { get; private set; }

        public ushort Id { get => Connection.Id; }

        public EventProvider OnPayloadReceivedEvent { get => Connection.OnPayloadReceived; }
        public EventProvider OnBufferReceivedEvent { get => Connection.OnBufferReceived; }
        public EventProvider OnConnectedEvent { get => Connection.OnConnected; }
        public EventProvider OnDisconnectedEvent { get => Connection.OnDisconnected; }
        public EventProvider OnBatchReceivedEvent { get; } = new EventProvider();

        public NetworkClientConnection(TcpTransportMode transportMode = TcpTransportMode.SendImmediate)
        {
            Connection = new TcpConnection(transportMode, 1);

            Connection.OnConnected.Add(OnConnected);
            Connection.OnDisconnected.Add(OnDisconnected);
            Connection.OnPayloadReceived.Add(OnPayload);
        }

        public void Connect(string address, int port) => Connection.Connect(address, port);
        public void Connect(IPAddress address, int port) => Connection.Connect(address, port);
        public void Connect(IPEndPoint address) => Connection.Connect(address);

        public void Disconnect() => Connection.Disconnect();

        public void Send(ArraySegment<byte> data) => Connection.Send(data);
        public void Send(params object[] data)
        {
            var batch = TransportHelper.NewBatch;

            foreach (var instance in data)
            {
                batch.WithData(instance);
            }

            Send(batch.ToSegment());
        }

        private void SendSynchronizationRequest() => Send(new SynchronizationRequestPacket(EndPoint.Address.ToString(), EndPoint.Port, PeerId.Next));

        private void OnPayload(EventArgsCollection eventArgsCollection)
        {
            var payload = eventArgsCollection.Get<TcpPayload>("payload");
            var batch = TransportHelper.NewBatch;

            batch.FromBytes(payload.Payload);

            if (batch.DataByType.Count > 0)
            {
                OnBatchReceivedEvent.Invoke(new KeyValuePair<string, object>("batch", batch));
            }
        }

        private void OnDisconnected(EventArgsCollection eventArgsCollection)
        {
            IsSynchronized = false;
        }

        private void OnConnected(EventArgsCollection eventArgsCollection)
        {
            if (!IsSynchronized) SendSynchronizationRequest();
        }
    }
}
