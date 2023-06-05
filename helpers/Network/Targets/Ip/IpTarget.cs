using System.Net;

namespace helpers.Network.Targets.Ip
{
    public class IpTarget : INetworkTarget
    {
        public string Address { get; }
        public int Port { get; }

        public IPAddress IPAddress { get; }
        public IPEndPoint EndPoint { get; }

        public bool IsLocal => IPAddress.IsLoopback(IPAddress) || IPAddress == IPAddress.Any;

        public IpTarget(string address, int port)
        {
            Address = address;
            Port = port;
            IPAddress = IpTargets.IsLocalTarget(address) ? IPAddress.Loopback : IPAddress.Parse(address);
            EndPoint = new IPEndPoint(IPAddress, Port);
        }

        public IpTarget(IPAddress address, int port)
        {
            EndPoint = new IPEndPoint(address, port);
            IPAddress = address;
            Port = port;
            Address = address.ToString();
        }

        public IpTarget(IPEndPoint iPEndPoint)
        {
            EndPoint = iPEndPoint;
            IPAddress = iPEndPoint.Address;
            Port = iPEndPoint.Port;
            Address = iPEndPoint.Address.ToString();
        }
    }
}