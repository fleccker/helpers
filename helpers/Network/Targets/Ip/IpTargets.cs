using System.Net;

namespace helpers.Network.Targets.Ip
{
    public static class IpTargets
    {
        public static bool IsLocalTarget(string address) => address != null && (address is "local" || address is "localhost" || address is "127.0.0.1" || address is "0.0.0.0");
        public static INetworkTarget GetLocalLoopback(int port = 0) => new IpTarget(IPAddress.Loopback, port);
        public static INetworkTarget GetLocalAny(int port = 0) => new IpTarget(IPAddress.Any, port);
        public static INetworkTarget Get(string address)
        {
            var ipParts = address.Split(':');
            var ip = ipParts[0];
            var port = int.Parse(ipParts[1]);

            return new IpTarget(ip, port);
        }
    }
}
