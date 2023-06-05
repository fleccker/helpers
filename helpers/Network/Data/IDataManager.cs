using helpers.Network.Features;

namespace helpers.Network.Data
{
    public interface IDataManager : INetworkFeature
    {
        DataMode Mode { get; }
        DataManagerSettings Settings { get; }

        long TotalBytesSent { get; }
        long TotalBytesReceived { get; }

        byte[] Serialize(DataPack data);
        DataPack Deserialize(byte[] data);
    }
}