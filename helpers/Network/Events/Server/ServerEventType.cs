namespace helpers.Network.Events.Server
{
    public enum ServerEventType
    {
        OnStarting,
        OnStarted,
        OnStopping,
        OnStopped,

        OnConnected,

        OnAuthentificating,
        OnAuthentificated,
        OnAuthentificationFailed,

        OnError,

        OnDataSent,
        OnDataReceived,

        OnDisconnected,
    }
}