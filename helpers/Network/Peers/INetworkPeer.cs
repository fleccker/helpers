using helpers.Network.Targets;
using helpers.Network.Features;
using helpers.Network.Controllers;
using helpers.Network.Data.InternalData;

using System;
using System.Collections.Generic;

using WatsonTcp;

namespace helpers.Network.Peers
{
    public interface INetworkPeer : INetworkObject
    {
        NetworkPeerStatus Status { get; }
        NetworkOperation Operation { get; }

        INetworkController Controller { get; }
        INetworkTarget Target { get; }
        INetworkFeatureManager Features { get; }

        Guid Id { get; }

        RemoteSpecifications Specifications { get; }

        bool IsAuthentificated { get; }

        void SetTarget(INetworkTarget target);
        void Connect();
        void Disconnect();
        void Disconnect(DisconnectReason reason);

        void Send(params object[] data);
        void Receive(IEnumerable<object> pack);
        void Receive(object data);
    }
}