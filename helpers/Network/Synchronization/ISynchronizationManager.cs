using helpers.Network.Features;

using System.Collections.Generic;

namespace helpers.Network.Synchronization
{
    public interface ISynchronizationManager : INetworkFeature
    {
        IReadOnlyList<ISynchronizationTarget> AllTargets { get; }

        bool Update(out SynchronizationData? data);
    }
}