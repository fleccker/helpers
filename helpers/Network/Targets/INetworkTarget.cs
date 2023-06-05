using System.Net;

namespace helpers.Network.Targets
{
    public interface INetworkTarget
    {
        string Address { get; }
        int Port { get; }

        IPAddress IPAddress { get; }
        IPEndPoint EndPoint { get; }

        bool IsLocal { get; }
    }
}