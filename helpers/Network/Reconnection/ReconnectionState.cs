namespace helpers.Network.Reconnection
{
    public enum ReconnectionState
    {
        Connected,

        Reconnecting,

        Cooldown,
        CooldownFailure
    }
}