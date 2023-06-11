using helpers.Extensions;
using helpers.Network.Authentification;
using helpers.Network.Controllers;
using helpers.Network.Targets;
using helpers.Network.Features;
using helpers.Network.Targets.Ip;
using helpers.Network.Callbacks;
using helpers.Network.Requests;
using helpers.Network.Data.InternalData;

using System;
using System.Collections.Generic;

using WatsonTcp;

namespace helpers.Network.Peers.Server
{
    public class ServerSidePeer : INetworkPeer
    {
        private Guid m_Id;

        private RemoteSpecifications m_Specs;

        private NetworkPeerStatus m_Status;
        private NetworkOperation m_Operation;

        private INetworkController m_Controller;
        private INetworkTarget m_Target;
        private INetworkFeatureManager m_Features;
        private IAuthentification m_Auth;

        public ServerSidePeer(INetworkController controller, ClientMetadata clientMetadata)
        {
            m_Id = clientMetadata.Guid;

            m_Status = NetworkPeerStatus.Connected;
            m_Operation = NetworkOperation.Receiving;

            m_Controller = controller;
            m_Target = IpTargets.Get(clientMetadata.IpPort);
            m_Features = new NetworkFeatureManager(this);
            m_Specs = null;

            m_Features.AddFeature<NetworkCallbackManager>();
            m_Features.AddFeature<RequestManager>();

            if (controller.RequiresAuthentification)
            {
                m_Auth = m_Features.AddFeature<KeyAuthentification>();
                m_Auth.Start(this);
            } 

            this.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.Send(new RemoteSpeficicationsRequest(), HandleSpecsResponse);
            });
        }

        public NetworkPeerStatus Status => m_Status;
        public NetworkOperation Operation => m_Operation;

        public INetworkController Controller => m_Controller;
        public INetworkTarget Target => m_Target;
        public INetworkFeatureManager Features => m_Features;

        public Guid Id => m_Id;

        public bool IsAuthentificated => m_Auth is null || m_Auth.Status is AuthentificationStatus.Authentificated;

        public RemoteSpecifications Specifications => m_Specs;

        public void Connect() => throw new InvalidOperationException();
        public void SetTarget(INetworkTarget target) => throw new InvalidOperationException();

        public void Disconnect()
        {
            if (Controller.Type is ControllerType.Client)
            {
                Controller.Disconnect(default);
            }
            else
            {
                Controller.Disconnect(Id);
            }
        }

        public void Disconnect(DisconnectReason reason)
        {
            if (Controller.Type is ControllerType.Client)
            {
                Controller.Disconnect(default, reason);
            }
            else
            {
                Controller.Disconnect(Id, reason);
            }
        }

        public void Receive(IEnumerable<object> pack) => pack.ForEach(Receive);
        public void Receive(object data)
        {
            if (!IsAuthentificated && !(data is IResponse))
            {
                Log.Warn($"Attempted to receive data of type {data} on an unauthentificated client.");
                return;
            }

            if (InternalProcess(data))
            {
                return;
            }

            var dataTargets = Features.GetDataTargets();
            if (dataTargets.Any())
            {
                foreach (var dataTarget in dataTargets)
                {
                    if (dataTarget.Accepts(data) && dataTarget.Process(data))
                    {
                        return;
                    }
                }
            }
        }

        public void Send(params object[] data) => Controller.Send(Id, data);

        internal void ProcessDisconnect()
        {
            m_Features.RemoveFeature<KeyAuthentification>();
            m_Features.RemoveFeature<NetworkCallbackManager>();
            m_Features.RemoveFeature<RequestManager>();
        }

        internal void HandleSpecsResponse(IResponse response)
        {
            response.RunIf<RemoteSpecifications>(specs =>
            {
                m_Specs = specs;
            });
        }

        private bool InternalProcess(object data) => false;
    }
}