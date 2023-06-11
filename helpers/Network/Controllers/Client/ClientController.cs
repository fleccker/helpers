using helpers.Extensions;
using helpers.Network.Authentification;
using helpers.Network.Callbacks;
using helpers.Network.Data;
using helpers.Network.Data.InternalData;
using helpers.Network.Data.Managers;
using helpers.Network.Events;
using helpers.Network.Events.Client;
using helpers.Network.Features;
using helpers.Network.Peers;
using helpers.Network.Reconnection;
using helpers.Network.Requests;
using helpers.Network.Targets;
using helpers.Network.Targets.Ip;
using helpers.Pooling.Pools;

using System;
using System.Collections.Generic;
using System.IO;

using WatsonTcp;

namespace helpers.Network.Controllers.Client
{
    public class ClientController : INetworkController
    {
        private WatsonTcpClient m_Client;

        private ClientEvents m_Events;
        private ClientEvents m_InternalEvents;

        private INetworkTarget m_Target;
        private INetworkFeatureManager m_Features;
        private IAuthentification m_Auth;
        private IDataManager m_DataManager;
        private IReconnector m_Reconnector;

        private string m_Key;

        public ClientController(string authKey = null)
        {
            m_Key = authKey;

            m_Events = new ClientEvents();
            m_InternalEvents = new ClientEvents();
            m_Target = IpTargets.GetLocalLoopback();
            m_Features = new NetworkFeatureManager(this, m_InternalEvents);
            m_Auth = authKey != null ? Features.AddFeature<KeyAuthentification>() : null;
            m_DataManager = Features.AddFeature<DataManager>();
            m_Reconnector = Features.AddFeature<Reconnector>();

            Features.AddFeature<NetworkCallbackManager>();
            Features.AddFeature<RequestManager>();

            this.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.ReplaceHandler<RemoteSpeficicationsRequest>(HandleRemoteSpecsRequest);
            });

            Peers = new List<INetworkPeer>();
        }

        public IReadOnlyList<INetworkPeer> Peers { get; }

        public INetworkPeer Peer => null;
        public INetworkEventCollection Events => m_Events;
        public INetworkFeatureManager Features => m_Features;
        public INetworkTarget Target { get => m_Target; set => m_Target = value; }
        public IReconnector Reconnector => m_Reconnector;
        public IDataManager DataManager => m_DataManager;
        public IAuthentification Authentification => m_Auth;

        public ControllerType Type => ControllerType.Client;

        public TimeSpan? UpTime => m_Client is null ? null : m_Client.Statistics.UpTime;

        public bool IsRunning => m_Client != null && m_Client.Connected;
        public bool IsAuthentificated => m_Auth is null || m_Auth.Status is AuthentificationStatus.Authentificated;
        public bool RequiresAuthentification => m_Auth != null;

        public string Key => m_Key;

        public void Connect()
        {
            if (Target is null) return;
            if (IsRunning) Stop();

            Start();
            ExecuteEvent(ClientEventType.OnConnecting, this, Target);

            m_Client.Connect();
        }

        public void Disconnect(Guid guid = default)
        {
            m_Client.Disconnect();
        }

        public void Disconnect(Guid guid, DisconnectReason reason)
        {
            m_Client.Disconnect();
        }

        public void Receive(object controllerData)
        {
            if (RequiresAuthentification && !IsAuthentificated && !(controllerData is IRequest))
            {
                Log.Warn($"Attempted to receive data on an unaunthentificated client ({controllerData}).");
                return;
            }

            if (InternalProcess(controllerData))
            {
                return;
            }

            var dataTargets = Features.GetDataTargets();
            if (dataTargets.Any())
            {
                foreach (var dataTarget in dataTargets)
                {
                    if (dataTarget.Accepts(controllerData) && dataTarget.Process(controllerData))
                    {
                        return;
                    }
                }
            }
        }

        public void Send(params object[] data)
        {
            if (!IsRunning) throw new InvalidOperationException();

            var pack = DataPackPool.Pool.Get();

            pack.Write(data);

            var bytes = DataManager.Serialize(pack);

            DataPackPool.Pool.Push(pack);

            m_Client.Send(bytes);
        }

        public void Send(Guid guid, params object[] data) => Send(data);

        public void Start()
        {
            m_Client = new WatsonTcpClient(Target.Address, Target.Port);
            m_Client.Settings.NoDelay = true;
            m_Client.Settings.DebugMessages = !Log.IsBlacklisted(LogLevel.Debug);
            m_Client.Settings.Logger = WatsonLog;

            RegisterWatsonEvents();
        }

        public void Stop()
        {
            if (m_Client is null) return;
            m_Client.Disconnect();

            UnregisterWatsonEvents();

            m_Client.Dispose();
            m_Client = null;
        }

        public bool TryGetPeer(Guid guid, out INetworkPeer peer)
        {
            peer = null;
            return false;
        }

        private bool InternalProcess(object data) => false;

        internal void ExecuteEvent(ClientEventType clientEventType, params object[] args)
        {
            m_InternalEvents.Execute(clientEventType, args);
            m_Events.Execute(clientEventType, args);
        }

        private void HandleRemoteSpecsRequest(IRequest request, RemoteSpeficicationsRequest remoteSpeficicationsRequest)
        {
            request.RespondSuccess(new RemoteSpecifications(
                CurrentSystem.OsName,
                CurrentSystem.OsType,
                CurrentSystem.OsServicePack,

                CurrentSystem.CpuName,

                CurrentSystem.CpuLogicalCores,
                CurrentSystem.CpuPhysicalCores,

                CurrentSystem.CpuFrequencyMHz,

                CurrentSystem.Bits));
        }

        private void RegisterWatsonEvents()
        {
            m_Client.Events.ServerDisconnected += OnServerDisconnected;
            m_Client.Events.ServerConnected += OnServerConnected;
            m_Client.Events.MessageReceived += OnMessageReceived;
            m_Client.Events.ExceptionEncountered += OnExceptionEncountered;
        }

        private void UnregisterWatsonEvents()
        {
            m_Client.Events.ServerDisconnected -= OnServerDisconnected;
            m_Client.Events.ServerConnected -= OnServerConnected;
            m_Client.Events.MessageReceived -= OnMessageReceived;
            m_Client.Events.ExceptionEncountered -= OnExceptionEncountered;
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

        private void OnExceptionEncountered(object sender, ExceptionEventArgs ev)
        {
            if (ev.Exception is null) return;
            if (ev.Exception is IOException) return;
            if (ev.Exception.InnerException != null && ev.Exception.InnerException is IOException) return;

            ExecuteEvent(ClientEventType.OnError, this, ev.Exception);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs ev)
        {
            var pack = DataManager.Deserialize(ev.Data);

            foreach (var data in pack.Pack)
            {
                ExecuteEvent(ClientEventType.OnDataReceived, this, data);
                Receive(data);
            }
        }

        private void OnServerConnected(object sender, ConnectionEventArgs ev)
        {
            Reconnector.Stop();
            ExecuteEvent(ClientEventType.OnConnected, this, Target);
        }

        private void OnServerDisconnected(object sender, DisconnectionEventArgs ev)
        {
            if (Reconnector.ShouldReconnect(ev.Reason))
                Reconnector.Start();

            ExecuteEvent(ClientEventType.OnDisconnected, this, ev.Reason);
        }
    }
}