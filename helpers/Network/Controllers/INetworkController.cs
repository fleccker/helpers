using helpers.Network.Events;
using helpers.Network.Peers;
using helpers.Network.Features;
using helpers.Network.Targets;
using helpers.Network.Authentification;

using System;
using System.Collections.Generic;

using WatsonTcp;

namespace helpers.Network.Controllers
{
    public interface INetworkController : INetworkObject
    {
        IReadOnlyList<INetworkPeer> Peers { get; }

        INetworkPeer Peer { get; }
        INetworkEventCollection Events { get; }
        INetworkFeatureManager Features { get; }
        INetworkTarget Target { get; set; }
        IAuthentification Authentification { get; }

        ControllerType Type { get; }

        TimeSpan? UpTime { get; }

        bool IsRunning { get; }
        bool RequiresAuthentification { get; }
        bool IsAuthentificated { get; }

        string Key { get; }

        bool TryGetPeer(Guid guid, out INetworkPeer peer);

        void Start();
        void Stop();

        void Connect();
        void Disconnect(Guid guid);
        void Disconnect(Guid guid, DisconnectReason reason);

        void Receive(object controllerData);

        void Send(params object[] data);
        void Send(Guid guid, params object[] data);
    }
}