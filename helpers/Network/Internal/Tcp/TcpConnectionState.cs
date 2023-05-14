namespace helpers.Network.Internal.Tcp
{
    public enum TcpConnectionState
    {
        Connected,
        Connecting,
        Disconnected,
        Disconnecting,
        NotInitialized,
        Initialized
    }
}
