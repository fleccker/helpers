namespace helpers.Network.Synchronization
{
    public interface ISynchronizationValidator
    {
        object ModeTarget { get; }

        SynchronizationMode SynchronizationMode { get; }
        ISynchronizationTarget SynchronizationTarget { get; }

        bool ShouldSynchonize();
    }
}