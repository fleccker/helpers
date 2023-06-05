namespace helpers.Network.Synchronization
{
    public interface ISynchronizationTarget
    {
        string Id { get; }

        ISynchronizationValidator Validator { get; }

        byte[] Synchronize();
    }
}