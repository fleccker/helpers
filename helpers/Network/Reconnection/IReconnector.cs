using helpers.Network.Features;

using System;

using WatsonTcp;

namespace helpers.Network.Reconnection
{
    public interface IReconnector : INetworkFeature
    {
        bool IsAllowed { get; }
        bool IsReconnecting { get; }

        int MaxAttempts { get; set; }
        int CurrentAttempts { get; }

        float CurrentDelay { get; }

        DateTime LastTry { get; }
        DateTime NextTry { get; }

        ReconnectionState State { get; }

        void Start();
        void Stop();

        bool ShouldReconnect(DisconnectReason reason);
    }
}