using helpers.Network.Extensions.Data;
using helpers.Network.Features;

using System.IO;

namespace helpers.Network.Data.Managers
{
    public class DataManager : NetworkFeatureBase, IDataManager
    {
        private long m_Sent = 0;
        private long m_Received = 0;

        private DataMode m_Mode = DataMode.SendImmediately;
        private DataManagerSettings m_Settings = new DataManagerSettings() { ReadSegments = true };

        public DataMode Mode => m_Mode;
        public DataManagerSettings Settings => m_Settings;

        public long TotalBytesSent => m_Sent;
        public long TotalBytesReceived => m_Received;

        public DataPack Deserialize(byte[] data)
        {
            m_Received += data.LongLength;

            DataPack pack = null;
            
            using (var memoryStream = new MemoryStream(data))
            using (var reader = new BinaryReader(memoryStream))
            {
                try
                {
                    pack = reader.ReadPack();
                }
                catch { }
            }

            if (pack != null)
            {
                return pack;
            }
            else
            {
                Log.Warn("Data Manager", "Failed to deserialize data pack!");
                return null;
            }
        }

        public byte[] Serialize(DataPack pack)
        {
            byte[] bytes = null;

            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write(pack);
                bytes = memoryStream.ToArray();
            }

            m_Sent += bytes.LongLength;

            return bytes;
        }
    }
}