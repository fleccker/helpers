namespace helpers.Network.Events.Client
{
    public enum ClientEventType
    {
        OnConnecting,
        OnConnected,

        OnAuthentificating,
        OnAuthentificated,
        OnAuthentificationFailed,

        OnError,

        OnDataSent,
        OnDataReceived,

        OnDisconnecting,
        OnDisconnected,

        OnReconnecting,
        OnReconnected,
        OnReconnectionFailed
    }
}