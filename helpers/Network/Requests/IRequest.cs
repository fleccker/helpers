using helpers.Network.Data;

using System;

namespace helpers.Network.Requests
{
    public interface IRequest : ISerializable
    {
        string Id { get; }
        object Request { get; }

        bool IsDeclined { get; }

        DateTime SentAt { get; }
        DateTime ReceivedAt { get; }

        INetworkObject AcceptedBy { get; }

        void Accept(INetworkObject networkObject);
        void RespondFail(object response = null);
        void RespondSuccess(object response);
        void Decline();
    }
}