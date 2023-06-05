using helpers.Random;

namespace helpers.Network.Requests
{
    public static class RequestExtensions
    {
        public static string RandomId => RandomGeneration.Default.GetReadableString(15);
    }
}