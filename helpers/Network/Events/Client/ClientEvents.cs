using helpers.Network.Authentification;
using helpers.Network.Data;
using helpers.Network.Reconnection;
using helpers.Network.Controllers;
using helpers.Network.Targets;
using helpers.Network.Peers;

using System;

using WatsonTcp;
using helpers.Extensions;

namespace helpers.Network.Events.Client
{
    public class ClientEvents : INetworkEventCollection
    {
        public event Action<INetworkController, INetworkTarget> OnConnecting;
        public event Action<INetworkController, INetworkTarget> OnConnected;

        public event Action<INetworkController, IAuthentificationData> OnAuthentificating;
        public event Action<INetworkController, IAuthentificationData> OnAuthentificated;
        public event Action<INetworkController, IAuthentificationData> OnAuthentificationFailed;

        public event Action<INetworkController, Exception> OnError;

        public event Action<INetworkController, object> OnDataReceived;

        public event Action<INetworkController, string> OnDisconnecting;
        public event Action<INetworkController, string> OnDisconnected;

        public event Action<INetworkController, IReconnector> OnReconnecting;
        public event Action<INetworkController, IReconnector, INetworkPeer> OnReconnected;
        public event Action<INetworkController, IReconnector> OnReconnectionFailed;

        internal void Execute(ClientEventType type, params object[] args)
        {
            switch (type)
            {
                case ClientEventType.OnConnecting:
                    OnConnecting?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkTarget>());
                    break;

                case ClientEventType.OnConnected:
                    OnConnected?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<INetworkTarget>());
                    break;

                case ClientEventType.OnAuthentificating:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IAuthentificationData>());
                    break;

                case ClientEventType.OnAuthentificated:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IAuthentificationData>());
                    break;

                case ClientEventType.OnAuthentificationFailed:
                    OnAuthentificating?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IAuthentificationData>());
                    break;

                case ClientEventType.OnError:
                    OnError?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<Exception>());
                    break;

                case ClientEventType.OnDataReceived:
                    OnDataReceived?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1]);
                    break;

                case ClientEventType.OnDisconnected:
                    OnDisconnected?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<DisconnectReason>().ToString());
                    break;

                case ClientEventType.OnDisconnecting:
                    OnDisconnecting?.Invoke(
                        args[0].As<INetworkController>(),  
                        args[1].As<DisconnectReason>().ToString());
                    break;

                case ClientEventType.OnReconnecting:
                    OnReconnecting?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IReconnector>());
                    break;

                case ClientEventType.OnReconnected:
                    OnReconnected?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IReconnector>(),
                        args[2].As<INetworkPeer>());

                    break;

                case ClientEventType.OnReconnectionFailed:
                    OnReconnectionFailed?.Invoke(
                        args[0].As<INetworkController>(),
                        args[1].As<IReconnector>());
                    break;
            }
        }
    }
}