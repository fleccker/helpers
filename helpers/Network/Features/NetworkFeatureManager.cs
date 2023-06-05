using helpers.Extensions;
using helpers.Network.Events;
using helpers.Network.Events.Server;
using helpers.Network.Controllers;
using helpers.Network.Data;

using System.Collections.Generic;

namespace helpers.Network.Features
{
    public class NetworkFeatureManager : INetworkFeatureManager
    {
        public NetworkFeatureManager(INetworkObject controller, INetworkEventCollection internalEvents = null)
        {
            m_Controller = controller;
            m_InternalEvents = internalEvents;

            if (m_InternalEvents != null && m_InternalEvents.Is<ServerEvents>(out var events))
            {
                events.OnStopping += OnStopping;
                events.OnStarted += OnStarted;
            }
        }

        private INetworkEventCollection m_InternalEvents;
        private INetworkObject m_Controller;
        private HashSet<INetworkFeature> m_Features = new HashSet<INetworkFeature>();

        public INetworkObject Controller => m_Controller;
        public IReadOnlyCollection<INetworkFeature> AllFeatures => m_Features;

        public TFeature AddFeature<TFeature>() where TFeature : INetworkFeature
        {
            var feature = Reflection.Instantiate<TFeature>();

            if (m_Features.Add(feature))
            {
                feature.OnInstalled(Controller, (feature.HasPriority && m_InternalEvents != null) || !(Controller is INetworkController) ? m_InternalEvents : (Controller as INetworkController).Events);
                feature.Enable();
                return feature;
            }
            else return GetFeature<TFeature>();
        }

        public TFeature GetFeature<TFeature>() where TFeature : INetworkFeature => m_Features.TryGetFirst<TFeature>(out var feature) ? feature : default;
        public IList<IDataTarget> GetDataTargets() => m_Features.Where<IDataTarget>();

        public bool RemoveFeature<TFeature>() where TFeature : INetworkFeature
        {
            while (m_Features.TryGetFirst<TFeature>(out var feature))
            {
                feature.Disable();
                feature.OnUninstalled(Controller, (feature.HasPriority && m_InternalEvents != null) || !(Controller is INetworkController) ? m_InternalEvents : (Controller as INetworkController).Events);
                m_Features.Remove(feature);
            }

            return true;
        }

        private void OnStopping(INetworkController controller)
        {
            m_Features.ForEach(x => x.Disable());
        }

        private void OnStarted(INetworkController controller)
        {
            m_Features.ForEach(x => x.Enable());
        }
    }
}