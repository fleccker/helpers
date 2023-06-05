using helpers.Network.Data;

using System;

namespace helpers.Network.Requests
{
    public interface IResponse : ISerializable
    {
        bool IsSuccess { get; }
        string RequestId { get; }
        object Response { get; }

        DateTime ReceivedAt { get; }
        DateTime SentAt { get; }
    }
}