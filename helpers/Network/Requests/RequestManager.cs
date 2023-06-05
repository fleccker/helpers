using helpers.Network.Controllers;
using helpers.Network.Features;
using helpers.Network.Peers;

using System;
using System.Collections.Generic;

namespace helpers.Network.Requests
{
    public class RequestManager : NetworkFeatureBase, IRequestManager
    {
        private int m_Sent = 0;
        private int m_Received = 0;

        private readonly Dictionary<string, Delegate> _awaitingResponse = new Dictionary<string, Delegate>();   
        private readonly Dictionary<Type, Delegate> _singletonHandlers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, List<Delegate>> _multipleHandlers = new Dictionary<Type, List<Delegate>>();

        public int Waiting => _awaitingResponse.Count;
        public int Sent => m_Sent;
        public int Received => m_Received;

        public override void OnDisabled()
        {
            m_Sent = 0;
            m_Received = 0;
            _awaitingResponse.Clear();
        }

        public override void OnEnabled() => OnDisabled();

        public bool Accepts(object data) => data != null && (data is IRequest || data is IResponse);
        public bool Process(object data)
        {
            if (data is null || (data is not IResponse && data is not IRequest)) return false;

            m_Received++;

            if (data is IResponse response)
            {
                if (_awaitingResponse.TryGetValue(response.RequestId, out var callback))
                {
                    callback.DynamicInvoke(response);
                }

                _awaitingResponse.Remove(response.RequestId);
                return true;
            }
            else
            {
                var request = data.As<IRequest>();
                var requestType = request.Request.GetType();

                request.Accept(Controller);

                if (_singletonHandlers.TryGetValue(requestType, out var singletonCallback))
                {
                    var callbackType = singletonCallback.GetType();
                    if (callbackType.FullName.StartsWith("System.Func"))
                    {
                        var responseCallback = singletonCallback.DynamicInvoke(request, request.Request);
                        if (responseCallback != null)
                        {
                            if (responseCallback is IResponse responseValue)
                            {
                                Send(responseValue, request);
                                return true;
                            }
                            else
                            {
                                Send(responseCallback, request, true);
                                return true;
                            }
                        }
                        else
                        {
                            Send(responseCallback, request, false);
                            return true;
                        }
                    }
                    else
                    {
                        singletonCallback.DynamicInvoke(request, request.Request);
                    }
                }

                if (_multipleHandlers.TryGetValue(requestType, out var handlers))
                {
                    handlers.ForEach(handler =>
                    {
                        var callbackType = handler.GetType();
                        if (callbackType.FullName.StartsWith("System.Func"))
                        {
                            var responseCallback = handler.DynamicInvoke(request, request.Request);
                            if (responseCallback != null)
                            {
                                if (responseCallback is IResponse responseValue)
                                {
                                    Send(responseValue, request);
                                    return;
                                }
                                else
                                {
                                    Send(responseCallback, request, true);
                                    return;
                                }
                            }
                            else
                            {
                                Send(responseCallback, request, false);
                                return;
                            }
                        }
                        else
                        {
                            handler.DynamicInvoke(request, request.Request);
                        }
                    });

                    return true;
                }
            }

            return false;
        }

        public void Send(IResponse response, IRequest request)
        {
            if (request.AcceptedBy is INetworkController controller && controller.Type != ControllerType.Server) controller.Send(response);
            else if (request.AcceptedBy is INetworkPeer peer) peer.Send(response);
            else throw new InvalidOperationException();

            m_Sent++;
        }

        public void Send(object request, Action<IResponse> callback) => Send(new RequestBase(RequestExtensions.RandomId, request), callback);
        public void Send(object response, IRequest request, bool isSuccess = true) => Send(new ResponseBase(isSuccess, request.Id, response), request);
        public void Send(IRequest request, Action<IResponse> callback)
        {
            _awaitingResponse[request.Id] = callback;

            this.ExecuteIfClient(controller => controller.Send(request));
            this.ExecuteIfServerPeer(peer => peer.Send(request));

            m_Sent++;
        }

        public void AddHandler<TRequest>(Action<IRequest, TRequest> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(TRequest))) _multipleHandlers.Add(typeof(TRequest), new List<Delegate>());
            _multipleHandlers[typeof(TRequest)].Add(handler);
        }

        public void AddHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(TRequest))) _multipleHandlers.Add(typeof(TRequest), new List<Delegate>());
            _multipleHandlers[typeof(TRequest)].Add(handler);
        }

        public void RemoveHandler<TRequest>(Action<IRequest, TRequest> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(TRequest))) _multipleHandlers.Add(typeof(TRequest), new List<Delegate>());
            _multipleHandlers[typeof(TRequest)].Remove(handler);
        }

        public void RemoveHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler)
        {
            if (!_multipleHandlers.ContainsKey(typeof(TRequest))) _multipleHandlers.Add(typeof(TRequest), new List<Delegate>());
            _multipleHandlers[typeof(TRequest)].Remove(handler);
        }

        public void ReplaceHandler<TRequest>(Action<IRequest, TRequest> handler)
        {
            _singletonHandlers[typeof(TRequest)] = handler;
        }

        public void ReplaceHandler<TRequest, TResponse>(Func<IRequest, TRequest, TResponse> handler)
        {
            _singletonHandlers[typeof(TRequest)] = handler;
        }
    }
}