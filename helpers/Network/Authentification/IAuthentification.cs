using helpers.Network.Features;
using helpers.Network.Peers;

using System;

namespace helpers.Network.Authentification
{
    public interface IAuthentification : INetworkFeature
    {
        IAuthentificationStorage Storage { get; }
        IAuthentificationData Data { get; }

        AuthentificationStatus Status { get; }
        AuthentificationFailureReason FailureReason { get; }

        DateTime SentAt { get; }
        DateTime ReceivedAt { get; }

        double TimeRequired { get; }

        void Start(INetworkPeer peer);
    }
}