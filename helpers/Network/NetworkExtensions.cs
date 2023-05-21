namespace helpers.Network
{
    public static class NetworkExtensions
    {
        public static bool IsLocalIp(string ip) => ip is "127.0.0.1" || ip is "0.0.0.0" || ip is "localhost" || ip is "local";
    }
}