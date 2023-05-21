using helpers.Events;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace helpers.Network.Internal.Tcp
{
    [LogSource("Tcp/Connection")]
    internal class TcpConnection
    {
        private CancellationToken? _sendToken;
        private CancellationToken? _receiveToken;

        internal CancellationTokenSource SendTokenSource { get; } = new CancellationTokenSource();
        internal CancellationTokenSource ReceiveTokenSource { get; } = new CancellationTokenSource();

        internal ConcurrentQueue<TcpPayload> SendQueue { get; } = new ConcurrentQueue<TcpPayload>();

        internal CancellationToken SendToken { get => _sendToken.HasValue ? _sendToken.Value : (_sendToken = SendTokenSource.Token).Value; }
        internal CancellationToken ReceiveToken { get => _receiveToken.HasValue ? _receiveToken.Value : (_receiveToken = ReceiveTokenSource.Token).Value; }

        internal IPEndPoint EndPoint { get; set; }

        internal Thread ConnectionThread { get; private set; }
        internal Thread TransportSendThread { get; private set; }
        internal Thread TransportReceiveThread { get; private set; }

        internal Timer TransportSendTimer { get; private set; }

        internal TcpClient Client { get; private set; }
        internal NetworkStream Stream { get; private set; }

        internal ushort Id { get; private set; }

        internal TcpTransportMode TransportMode { get; private set; }
        internal TcpConnectionState State { get; private set; }
        internal TcpConnectionState ActualState
        {
            get
            {
                if (Client != null)
                {
                    if (Client.Connected) return TcpConnectionState.Connected;
                }

                return TcpConnectionState.Disconnected;
            }
        }

        internal readonly EventProvider OnBufferReceived = new EventProvider();
        internal readonly EventProvider OnPayloadReceived = new EventProvider();
        internal readonly EventProvider OnConnected = new EventProvider();
        internal readonly EventProvider OnDisconnected = new EventProvider();

        internal TcpConnection(TcpTransportMode tcpTransportMode, int transportPollInterval = 0)
        {
            Client = new TcpClient();
            State = TcpConnectionState.Initialized;
            TransportMode = tcpTransportMode;

            TransportReceiveThread = new Thread(StartReceive);
            TransportSendThread = new Thread(StartReceive);
            TransportSendTimer = new Timer(StartSend, null, Timeout.Infinite, transportPollInterval);

            Id = PeerId.Next;

            OnBufferReceived.Add(OnBuffer);
        }

        internal TcpConnection(TcpClient client, ushort id, TcpTransportMode tcpTransportMode)
        {
            Client = client;
            Stream = client.GetStream();
            State = TcpConnectionState.Connected;
            Id = id;
            TransportMode = tcpTransportMode;

            Configure(x =>
            {
                x.Blocking = false;
                x.DontFragment = true;
                x.DualMode = false;
                x.EnableBroadcast = false;
                x.ExclusiveAddressUse = false;
            });

            OnBufferReceived.Add(OnBuffer);
        }

        [LogSource("Tcp/Connection/Connect/4")]
        public void Connect(IPEndPoint endPoint)
        {
            EndPoint = endPoint;

            Log.Debug($"Connection EndPoint set to: {EndPoint}");

            Connect();
        }

        [LogSource("Tcp/Connection/Connect/3")]
        public void Connect(IPAddress address, int port) => Connect(new IPEndPoint(address, port));

        [LogSource("Tcp/Connection/Connect/2")]
        public void Connect(string address, int port)
        {
            Log.Debug($"Connecting to {address}:{port} ..");

            if (NetworkExtensions.IsLocalIp(address))
                Connect(IPAddress.Loopback, port);
            else
            {
                if (!IPAddress.TryParse(address, out var iPAddress)) throw new ArgumentException($"{address} is not a valid IP address!");
                Connect(iPAddress, port);
            }
        }

        [LogSource("Tcp/Connection/Configure")]
        public void Configure(Action<Socket> configure) => configure?.Invoke(Client?.Client);

        [LogSource("Tcp/Connection/Connect/0")]
        public void Connect()
        {
            Log.Debug($"Connecting ..");

            State = TcpConnectionState.Connecting;

            Log.Debug("Creating the connection thread ..");

            ConnectionThread = new Thread(() =>
            {
                Log.Debug($"Connection thread started!");

                Client.Connect(EndPoint);

                Log.Debug($"Connected!");

                State = TcpConnectionState.Connected;
                ConnectionThread = null;

                Log.Debug("Configuring ..");
                Configure(x =>
                {
                    x.Blocking = false;
                    x.DontFragment = true;
                    x.DualMode = false;
                    x.EnableBroadcast = false;
                    x.ExclusiveAddressUse = false;
                });
                Log.Debug("Configured!");

                Log.Debug("Retrieving network stream ..");
                Stream = Client.GetStream();
                Log.Debug("Network stream retrieved!");

                OnConnected.Invoke();
            });

            Log.Debug($"Starting the connection thread ..");

            ConnectionThread.Start();
        }

        [LogSource("Tcp/Connection/Disconnect")]
        public void Disconnect()
        {
            Stream.Close();
            Client.Dispose();
            Stream.Dispose();

            Client = null;
            Stream = null;

            State = TcpConnectionState.Disconnected;

            TransportSendTimer.Dispose();
            ReceiveTokenSource.Cancel();
            SendTokenSource.Cancel();
            ReceiveTokenSource.Dispose();
            SendTokenSource.Dispose();

            OnDisconnected.Invoke();
        }

        [LogSource("Tcp/Connection/Send/1")]
        public void Send(ArraySegment<byte> data)
        {
            if (TransportMode is TcpTransportMode.SendImmediate)
            {
                if (Stream != null)
                {
                    var payload = new TcpPayload(Id, MessageId.Next, data.Array, Client.Client.LocalEndPoint);
                    var bytes = payload.ToBytes();

                    Log.Debug($"Writing {bytes.Length} bytes");

                    Stream.Write(bytes, 0, bytes.Length);

                    Log.Debug($"Written {bytes.Length} bytes");
                }
            }
            else
            {
                Log.Debug($"Enqueued data: {data.Count}");

                SendQueue.Enqueue(new TcpPayload(Id, MessageId.Next, data.Array, Client.Client.LocalEndPoint));
            }
        }

        [LogSource("Tcp/Connection/StartSend")]
        internal void StartSend(object state)
        {
            Log.Debug("Flushing the send queue ..");

            while (SendQueue.TryDequeue(out TcpPayload payload))
            {
                Log.Debug($"Payload: {payload.PeerId} / {payload.MessageId}");

                var bytes = payload.ToBytes();

                Log.Debug($"Payload size: {bytes.Length} / {payload.Payload.Length} / {payload.SourceBytes.Length}");

                Stream?.Write(bytes, 0, bytes.Length);

                Log.Debug("Payload written to stream");
            }

            Log.Debug("Send queue flushed!");
        }

        [LogSource("Tcp/Connection/StartReceive")]
        internal void StartReceive()
        {
            while (true)
            {
                ReceiveToken.ThrowIfCancellationRequested();

                if (Stream != null)
                {
                    byte[] outputBuffer = new byte[8096];

                    Log.Debug($"Output buffer initialized: {outputBuffer.Length}");

                    while (Stream.Read(outputBuffer, 0, outputBuffer.Length) > 0)
                    {
                        Log.Debug($"Read bytes!");
                        continue;
                    }

                    Log.Debug($"Read {outputBuffer.Length} bytes");

                    OnBufferReceived.Invoke(new KeyValuePair<string, object>("buffer", outputBuffer));
                }
            }
        }

        [LogSource("Tcp/Connection/OnBuffer")]
        private void OnBuffer(EventArgsCollection argsCollection)
        {
            var buffer = argsCollection.Get<byte[]>("buffer");
            var payload = JsonConvert.DeserializeObject<TcpPayload>(Encoding.ASCII.GetString(buffer));

            OnPayloadReceived.Invoke(new KeyValuePair<string, object>("payload", payload));
        }
    }
}