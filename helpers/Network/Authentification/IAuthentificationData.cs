using helpers.Network.Data;

namespace helpers.Network.Authentification
{
    public interface IAuthentificationData : ISerializable
    {
        string ClientKey { get; }
    }
}