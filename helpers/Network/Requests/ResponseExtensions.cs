using System;

namespace helpers.Network.Requests
{
    public static class ResponseExtensions
    {
        public static void RunIf<TCast>(this IResponse response, Action<TCast> action)
        {
            if (response.TryCast<TCast>(out var cast))
            {
                action?.Invoke(cast);
            }
        }

        public static bool TryCast<TCast>(this IResponse response, out TCast cast)
        {
            if (!response.IsSuccess || response.Response is null)
            {
                cast = default;
                return false;
            }

            if (!(response.Response is TCast castVar) || castVar is null)
            {
                cast = default;
                return false;
            }

            cast = castVar;
            return true;
        }

        public static TCast Cast<TCast>(this IResponse response)
        {
            if (!response.IsSuccess) return default;
            if (response.Response is null) return default;
            if (!(response.Response is TCast cast)) return default;
            if (cast is null) return default;

            return cast;
        }
    }
}