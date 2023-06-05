using helpers.Network.Events;

namespace helpers.Network.Features
{
    public interface INetworkFeature
    {
        INetworkObject Controller { get; }

        bool HasPriority { get; }

        void OnInstalled(INetworkObject controller, INetworkEventCollection networkEventCollection);
        void OnUninstalled(INetworkObject controller, INetworkEventCollection networkEventCollection);

        void Disable();
        void Enable();

        bool Receive(object data);
    }
}