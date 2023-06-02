using helpers.Events;
using helpers.Extensions;
using helpers.Timeouts;

using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;

using WatsonTcp;

namespace helpers.Network
{
    public class NetworkClient
    {
        private volatile WatsonTcpClient _client;
        private volatile bool _connecting;
        private int _curAttempts = 0;
        private NetworkTransport _transport;
        private readonly RepeatedHandle _sendHandle;
        private RepeatedHandle _reconnectHandle;
        private IPEndPoint _lastEndPoint;

        private readonly Dictionary<Type, EventProvider> _typeEvents = new Dictionary<Type, EventProvider>();
        private readonly Dictionary<Type, HashSet<Action<object>>> _tempHandlers = new Dictionary<Type, HashSet<Action<object>>>();

        public IPAddress Address => IsActive ? IPAddress.Parse(Reflection.GetField<string>(typeof(WatsonTcpClient), "_ServerIp", _client)) : null;
        public int Port => IsActive ? Reflection.GetField<int>(typeof(WatsonTcpClient), "_ServerPort", _client) : -1;

        public int ReconnectionAttempts { get; set; } = 5;
        public float ReconnectionDelay { get => _reconnectHandle.Delay; set => _reconnectHandle.Delay = value; }

        public bool IsActive => _client != null && _client.Connected;

        public readonly EventProvider OnReceived = new EventProvider();
        public readonly EventProvider OnConnecting = new EventProvider();
        public readonly EventProvider OnConnected = new EventProvider();
        public readonly EventProvider OnDisconnected = new EventProvider();

        public NetworkClient()
        {
            _transport = new NetworkTransport();
            _sendHandle = new RepeatedHandle(1.5f, SendAll);
            _reconnectHandle = new RepeatedHandle(3f, Reconnect);
        }

        public EventProvider GetProviderForType(Type type)
        {
            if (!_typeEvents.ContainsKey(type)) _typeEvents.Add(type, new EventProvider());
            return _typeEvents[type];
        }

        public void AddHandler<TType>(Action<TType> handler)
        {
            if (!_tempHandlers.ContainsKey(typeof(TType))) _tempHandlers.Add(typeof(TType), new HashSet<Action<object>>());
            _tempHandlers[typeof(TType)].Add(x =>
            {
                if (!(x is TType t)) return;
                handler?.Invoke(t);
            });
        }

        public void Connect(string address, int port)
        {
            if (address is "localhost" || address is "local" || address is "127.0.0.1" || address is "0.0.0.0") Connect(IPAddress.Loopback, port);
            else Connect(IPAddress.Parse(address), port);
        }

        public void Connect(IPAddress address, int port) => Connect(new IPEndPoint(address, port));
        public void Connect(IPEndPoint endPoint)
        {
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Connecting to {endPoint}");

            if (IsActive) Disconnect();

            _client = new WatsonTcpClient(endPoint.Address.ToString(), endPoint.Port);
            _lastEndPoint = endPoint;

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Created client");
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Connecting ..");
            Log.Info($"Network Client [{_lastEndPoint}]", $"Connecting to {endPoint} ..");

            RegisterEvents();

            new Thread(() =>
            {
                OnConnecting.Invoke();
                _sendHandle.Pause();
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Send handle paused");
                _connecting = true;
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Client connecting");
                _client.Connect();
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Client connected");
                _sendHandle.Resume();
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Send handle resumed");
                _connecting = false;
            }).Start();
        }

        public void Disconnect()
        {
            if (!IsActive) throw new InvalidOperationException($"The server is not active!");

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Disconnecting");

            _sendHandle.Pause();
            _sendHandle.Execute();
            _sendHandle.Stop();

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Stopped send handle");

            _reconnectHandle.Stop();

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Stopped reconnect handle");

            UnregisterEvents();

            _client.Disconnect();
            _client.Dispose();
            _client = null;

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Disposed client");

            _transport.ClearRead();
            _transport.ClearSaved();
            _transport = null;

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Cleared transport");

            _typeEvents.Clear();
        }

        public void Send(bool immediate = false, params object[] data)
        {
            if (!IsActive) throw new InvalidOperationException($"Cannot send from unconnected client.");
            if (!immediate)
            {
                lock (_transport)
                { 
                    data.ForEach(x => _transport.Write(x));
                    Log.Debug($"Network Client [{_lastEndPoint}]", $"Written data");
                }
            }
            else
            {
                var transport = new NetworkTransport();
                data.ForEach(transport.Write);
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Written immediate data");
                _client.Send(transport.Combine());
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Send data");
            }
        }

        private void RegisterEvents()
        {
            _client.Events.ServerDisconnected += OnServerDisconnected;
            _client.Events.ServerConnected += OnServerConnected;
            _client.Events.ExceptionEncountered += OnException;
            _client.Events.MessageReceived += OnMessage;

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Registered events.");
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Received {e.Data.Length} bytes");

            var transport = new NetworkTransport(e.Data);

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Created temporary transport: {transport.ReadData.Count}");

            foreach (var obj in transport.ReadData)
            {
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Calling OnReceived for {obj} ({transport.ReadData.Count})");

                OnReceived.Invoke(
                    new KeyValuePair<string, object>("data", obj),
                    new KeyValuePair<string, object>("client", this),
                    new KeyValuePair<string, object>("transport", transport));

                if (obj != null)
                {
                    var type = obj.GetType();
                    if (_typeEvents.TryGetValue(type, out var provider)) 
                        provider.Invoke(
                            new KeyValuePair<string, object>("data", obj),
                            new KeyValuePair<string, object>("client", this),
                            new KeyValuePair<string, object>("transport", transport));

                    if (_tempHandlers.TryGetValue(type, out var handlers)) handlers.ForEach(x => x.Invoke(obj));
                    try { _tempHandlers[type].Clear(); } catch { }
                }
            }

            transport.ClearRead();
            transport = null;

            Log.Debug($"Network Client [{_lastEndPoint}]", $"Cleared temporary transport");
        }

        private void OnException(object sender, ExceptionEventArgs e)
        {
            Log.Error($"Network Client [{Address}]", $"Encountered an exception!");
            Log.Error($"Network Client [{Address}]", $"{e.Exception}");
        }

        private void OnServerConnected(object sender, ConnectionEventArgs e)
        {
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Connected: {e.Client.IpPort}");
            OnConnected.Invoke(e.Client.Guid, e.Client.IpPort);
            Log.Info($"Network Client [{Address}]", $"Connected to {e.Client.IpPort} ({e.Client.Guid})!");
        }

        private void OnServerDisconnected(object sender, DisconnectionEventArgs e)
        {
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Disconnected: {e.Reason}");

            OnDisconnected.Invoke(e.Reason.ToString());

            if (e.Reason is DisconnectReason.Shutdown) Log.Info($"Network Client [{Address}]", $"Client disconnected - the server is shutting down.");
            else if (e.Reason is DisconnectReason.Timeout) Log.Warn($"Network Client [{Address}]", $"Client disconnected - timed out!");
            else if (e.Reason is DisconnectReason.Removed) Log.Warn($"Network Client [{Address}]", $"Client disconnected - removed!");
            else Log.Warn($"Network Client [{Address}]", $"Client disconnected (other) - {e.Reason}");

            if ((e.Reason is DisconnectReason.Timeout || e.Reason is DisconnectReason.Removed) && ReconnectionAttempts > 0)
            {
                _reconnectHandle.Resume();
                Log.Debug($"Network Client [{_lastEndPoint}]", $"Resumed reconnect handle");
            }
        }

        private void UnregisterEvents()
        {
            _client.Events.ServerDisconnected -= OnServerDisconnected;
            _client.Events.ServerConnected -= OnServerConnected;
            _client.Events.ExceptionEncountered -= OnException;
            _client.Events.MessageReceived -= OnMessage;
            Log.Debug($"Network Client [{_lastEndPoint}]", $"Unregistered events.");
        }

        private void SendAll()
        {
            if (!IsActive) return;

            lock (_transport)
            {
                _client.Send(_transport.Combine());
                _transport.ClearSaved();          
                Log.Debug($"Network Client [{_lastEndPoint}]", $"SendAll");
            }
        }

        private void Reconnect()
        {
            if (_lastEndPoint is null) return;
            if (_connecting) return;
            if (_curAttempts >= ReconnectionAttempts) return;

            Log.Info($"Network Client [{_lastEndPoint}]", $"Reconnecting ..");

            _curAttempts++;
            Connect(_lastEndPoint);
        }
    }
}