using helpers.Network.Features;
using helpers.Network.Requests;

using System;

namespace helpers.Network.Controllers
{
    public static class ControllerExtensions
    {
        public static void Request(this INetworkObject client, object request, Action<IResponse> response)
        {
            Log.Debug($"Requesting {request} from {client}");

            client.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.Send(request, response);
            });
        }

        public static void Request<TResponse>(this INetworkController client, object request, Action<TResponse> response) where TResponse : IResponse
        {
            Log.Debug($"Requesting {request} from {client}");

            client.ExecuteFeature<IRequestManager>(requests =>
            {
                requests.Send(request, Reflection.TypeProxy<IResponse, TResponse>(response, true));
            });
        }
    }
}