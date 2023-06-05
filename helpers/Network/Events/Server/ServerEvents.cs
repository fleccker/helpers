using helpers.Network.Authentification;
using helpers.Network.Controllers;
using helpers.Network.Peers;
using helpers.Network.Targets;

using System;

using WatsonTcp;
using helpers.Extensions;

namespace helpers.Network.Events.Server
{
    public class ServerEvents : INetworkEventCollection
    {
        public event Action<INetworkController> OnStarting;
        public event Action<INetworkController> OnStopping;
        public event Action<INetworkController> OnStarted;
        public event Action<INetworkController> OnStopped;

        public event Action<INetworkController, INetworkTarget, INetworkPeer> OnConnected;

        public event Action<INetworkController, INetworkPeer, IAuthentificationData> OnAuthentificating;
        public event Action<INetworkController, INetworkPeer, IAuthentificationData> OnAuthentificated;
        public event Action<INetworkController, INetworkPeer, IAuthentificationData> OnAuthentificationFailed;

        public event Action<INetworkController, Exception> OnError;

        public event Action<INetworkController, INetworkPeer, object> OnDataReceived;

        public event Action<INetworkController, INetworkPeer, string> OnDisconnected;

        internal void Execute(ServerEventType type, params object[] args)
        {
            switch (type)
            {
                case ServerEventType.OnStarting:
                    OnStarting?.Invoke(
                        args[0].As<INetworkController>());
                    break;

                case ServerEventType.OnStarted:
                    OnStarted?.Invoke(
                        args[0].As<INetworkController>());
                    break;

                case ServerEventType.OnStopping:
                    OnStopping?.Invoke(
                        args[0].As<INetworkController>());
                    break;

                case ServerEventType.OnStopped:
                    OnStopped?.Invoke(
                        args[0].As<INetworkController>());
                    break;

                case ServerEventType.OnConnected:
                    OnConnected?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkTarget>(),
                        args[2].As<INetworkPeer>());
                    break;

                case ServerEventType.OnAuthentificating:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkPeer>(),
                        args[2].As<IAuthentificationData>());
                    break;

                case ServerEventType.OnAuthentificated:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkPeer>(),
                        args[2].As<IAuthentificationData>());
                    break;

                case ServerEventType.OnAuthentificationFailed:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkPeer>(),
                        args[2].As<IAuthentificationData>());
                    break;

                case ServerEventType.OnError:
                    OnError?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<Exception>());
                    break;

                case ServerEventType.OnDataReceived:
                    OnDataReceived?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkPeer>(),
                        args[2]);
                    break;

                case ServerEventType.OnDisconnected:
                    OnDisconnected?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkPeer>(),
                        args[2].As<DisconnectReason>().ToString());
                    break;
            }
        }
    }
}