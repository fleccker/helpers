using helpers.Network.Events;

namespace helpers.Network.Features
{
    public class NetworkFeatureBase : INetworkFeature
    {
        private INetworkObject m_Controller;
        private bool m_Enabled;

        public INetworkObject Controller => m_Controller;

        public virtual bool HasPriority { get; }
        public virtual bool IsEnabled => m_Enabled;

        public virtual void Install(INetworkEventCollection networkEventCollection) { }
        public virtual void Uninstall(INetworkEventCollection networkEventCollection) { }
        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }
        public virtual bool Receive(object data) { return false; }

        void INetworkFeature.Disable()
        {
            m_Enabled = false;
            OnDisabled();
        }

        void INetworkFeature.Enable()
        {
            m_Enabled = true;
            OnEnabled();
        }

        void INetworkFeature.OnInstalled(INetworkObject controller, INetworkEventCollection networkEventCollection)
        {
            m_Controller = controller;
            Install(networkEventCollection);
        }

        void INetworkFeature.OnUninstalled(INetworkObject controller, INetworkEventCollection networkEventCollection)
        {
            Uninstall(networkEventCollection);
            m_Controller = null;
        }
    }
}