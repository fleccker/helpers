using helpers.Network.Controllers;
using helpers.Network.Peers;

using System;

namespace helpers.Network.Features
{
    public static class FeatureExtensions
    {
        public static void ExecuteFeature<TFeature>(this INetworkObject networkObject, Action<TFeature> action) where TFeature : INetworkFeature
        {
            var feature = networkObject.GetFeature<TFeature>();
            if (feature is null) return;

            action?.Invoke(feature);
        }

        public static TFeature GetFeature<TFeature>(this INetworkObject networkObject) where TFeature : INetworkFeature
        {
            var features = networkObject.GetFeatures();
            if (features is null) return default;
            return features.GetFeature<TFeature>();
        }

        public static INetworkFeatureManager GetFeatures(this INetworkObject networkObject)
        {
            if (networkObject is INetworkPeer peer) return peer.Features;
            if (networkObject is INetworkController controller) return controller.Features;

            return null;
        }

        public static void ExecuteIfServerPeer(this INetworkFeature feature, Action<INetworkPeer> execute)
        {
            if (feature.Controller is INetworkPeer peer && peer.Controller.Type is ControllerType.Server) 
                execute?.Invoke(peer);
        }

        public static void ExecuteIfServer(this INetworkFeature feature, Action<INetworkController> execute)
        {
            if (feature.Controller is INetworkController controller && controller.Type is ControllerType.Server)
                execute?.Invoke(controller);
        }

        public static void ExecuteIfClient(this INetworkFeature feature, Action<INetworkController> execute)
        {
            if (feature.Controller is INetworkController controller && controller.Type is ControllerType.Client)
                execute?.Invoke(controller);
        }
    }
}