using helpers.Network.Events;
using helpers.Network.Events.Server;
using helpers.Network.Features;
using helpers.Network.Authentification;
using helpers.Network.Peers.Server;
using helpers.Network.Targets.Ip;
using helpers.Network.Data;
using helpers.Network.Data.Managers;
using helpers.Network.Peers;
using helpers.Network.Callbacks;
using helpers.Network.Targets;
using helpers.Pooling.Pools;
using helpers.Extensions;

using System.Collections.Generic;
using System.IO;
using System;

using WatsonTcp;

namespace helpers.Network.Controllers.Server
{
    public class ServerController : INetworkController
    {
        private List<ServerSidePeer> m_Peers = new List<ServerSidePeer>();

        private IDataManager m_DataManager;
        private INetworkCallbackManager m_Callbacks;
        private INetworkFeatureManager m_Features;
        private WatsonTcpServer m_Server;

        private ServerEvents m_InternalEvents = new ServerEvents();
        private ServerEvents m_ExternalEvents = new ServerEvents();

        private IAuthentification m_Auth;

        public ServerController(bool useAuth = false)
        {
            m_Features = new NetworkFeatureManager(this);
            m_Auth = useAuth ? m_Features.AddFeature<KeyAuthentification>() : null;
            m_DataManager = m_Features.AddFeature<DataManager>();
        }

        public IReadOnlyList<INetworkPeer> Peers => m_Peers;

        public INetworkPeer Peer => null;
        public INetworkEventCollection Events => m_ExternalEvents;
        public INetworkFeatureManager Features => m_Features;
        public INetworkCallbackManager Callbacks => m_Callbacks;
        public IDataManager DataManager => m_DataManager;
        public IAuthentification Authentification => m_Auth;
        public INetworkTarget Target { get; set; } = IpTargets.GetLocalLoopback();

        public ControllerType Type => ControllerType.Server;

        public TimeSpan? UpTime => m_Server != null ? m_Server.Statistics.UpTime : null;

        public bool IsRunning => m_Server != null && m_Server.IsListening;
        public bool IsAuthentificated => true;
        public bool RequiresAuthentification => m_Auth != null;

        public string Key => null;

        public void Start()
        {
            if (IsRunning) throw new InvalidOperationException($"The server is already running!");

            m_Server = new WatsonTcpServer(Target.IsLocal ? null : Target.Address, Target.Port);
            m_Server.Settings.NoDelay = true;
            m_Server.Settings.DebugMessages = !Log.IsBlacklisted(LogLevel.Debug);
            m_Server.Settings.Logger = WatsonLog;

            ExecuteEvent(ServerEventType.OnStarting, this);
            RegisterWatsonEvents();

            m_Server.Start();

            ExecuteEvent(ServerEventType.OnStarted, this);
        }

        public void Stop()
        {
            if (!IsRunning) throw new InvalidOperationException($"The server is not running!");

            ExecuteEvent(ServerEventType.OnStopping, this);

            m_Server.DisconnectClients(MessageStatus.Shutdown, true);
            m_Server.Stop();
            m_Server.Settings.Logger = null;

            UnregisterWatsonEvents();

            m_Server.Dispose();
            m_Server = null;

            ExecuteEvent(ServerEventType.OnStopped, this);
        }

        public void Receive(object data) => throw new InvalidOperationException();
        public void Connect() => throw new InvalidOperationException();
        public void Send(params object[] data) => throw new InvalidOperationException();

        public bool TryGetPeer(Guid guid, out INetworkPeer peer) => m_Peers.TryGetFirst(x => x.Id == guid, out peer); 

        public void Disconnect(Guid guid)
        {
            m_Server.DisconnectClient(guid);
        }

        public void Disconnect(Guid guid, DisconnectReason reason)
        {
            m_Server.DisconnectClient(guid, ToMessageStatus(reason));
        }

        public void Send(Guid guid, params object[] data)
        {
            if (!IsRunning) throw new InvalidOperationException($"The server is not running!");

            var pack = DataPackPool.Pool.Get();

            pack.Write(data);

            var bytes = DataManager.Serialize(pack);

            m_Server.Send(guid, bytes);

            DataPackPool.Pool.Push(pack);
        }

        internal void ExecuteEvent(ServerEventType serverEventType, params object[] args)
        {
            m_InternalEvents.Execute(serverEventType, args);
            m_ExternalEvents.Execute(serverEventType, args);
        }

        private void RegisterWatsonEvents()
        {
            m_Server.Events.ServerStarted += OnServerStarted;
            m_Server.Events.ServerStopped += OnServerStopped;
            m_Server.Events.ClientDisconnected += OnClientDisconnected;
            m_Server.Events.ClientConnected += OnClientConnected;
            m_Server.Events.MessageReceived += OnMessageReceived;
            m_Server.Events.ExceptionEncountered += OnExceptionEncountered;
        }

        private void UnregisterWatsonEvents()
        {
            m_Server.Events.ServerStarted -= OnServerStarted;
            m_Server.Events.ServerStopped -= OnServerStopped;
            m_Server.Events.ClientDisconnected -= OnClientDisconnected;
            m_Server.Events.ClientConnected -= OnClientConnected;
            m_Server.Events.MessageReceived -= OnMessageReceived;
            m_Server.Events.ExceptionEncountered -= OnExceptionEncountered;
        }

        private void WatsonLog(Severity severity, string message)
        {
            switch (severity)
            {
                case Severity.Warn:
                case Severity.Alert:
                    Log.Warn("Watson", message);
                    break;

                case Severity.Error:
                case Severity.Critical:
                case Severity.Emergency:
                    Log.Error("Watson", message);
                    break;

                case Severity.Debug:
                    Log.Debug("Watson", message);
                    break;

                case Severity.Info:
                    Log.Info("Watson", message);
                    break;
            }
        }

        private void OnExceptionEncountered(object sender, ExceptionEventArgs e)
        {
            if (e.Exception is null) return;
            if (e.Exception is IOException) return;
            if (e.Exception.InnerException is not null && e.Exception.InnerException is IOException) return;

            ExecuteEvent(ServerEventType.OnError, this, e.Exception);

            Log.Error("Network Server", $"Exception encountered:\n{e.Exception}");
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var pack = DataManager.Deserialize(e.Data);
            var peerFound = TryGetPeer(e.Client.Guid, out var peer);

            pack.Pack.ForEach(x =>
            {
                ExecuteEvent(ServerEventType.OnDataReceived, this, peer, x);
                if (peerFound) peer.Receive(x);
            });
        }

        private void OnClientConnected(object sender, ConnectionEventArgs e)
        {
            var peer = new ServerSidePeer(this, e.Client);
            m_Peers.Add(peer);
            ExecuteEvent(ServerEventType.OnConnected, this, peer.Target, peer);
        }

        private void OnClientDisconnected(object sender, DisconnectionEventArgs e)
        {
            if (TryGetPeer(e.Client.Guid, out var peer))
            {
                (peer as ServerSidePeer).ProcessDisconnect();
                ExecuteEvent(ServerEventType.OnDisconnected, this, peer, e.Reason);
            }

            var removed = m_Peers.RemoveAll(x => x.Id == e.Client.Guid);

            Log.Info("Network Server", $"Client disconnected: {e.Client.Guid} ({e.Client.IpPort})");
        }

        private void OnServerStopped(object sender, EventArgs e)
        {
            m_Peers.ForEach(x => x.ProcessDisconnect());
            m_Peers.Clear();
            ExecuteEvent(ServerEventType.OnStopped, this);
        }

        private void OnServerStarted(object sender, EventArgs e)
        {
            ExecuteEvent(ServerEventType.OnStarted, this);
        }

        private MessageStatus ToMessageStatus(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.AuthFailure:
                    return MessageStatus.AuthFailure;
                case DisconnectReason.Timeout:
                    return MessageStatus.Timeout;
                case DisconnectReason.Removed:
                    return MessageStatus.Removed;
                case DisconnectReason.Shutdown:
                    return MessageStatus.Shutdown;
                case DisconnectReason.Normal:
                    return MessageStatus.Normal;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
