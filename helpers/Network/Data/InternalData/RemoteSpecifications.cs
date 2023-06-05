using System.IO;

namespace helpers.Network.Data.InternalData
{
    public class RemoteSpecifications : SerilizableBase
    {
        public string OsName;
        public string OsType;
        public string OsPack;

        public string CpuName;

        public int CpuLCores;
        public int CpuPCores;

        public double CpuSpeed;

        public int BitSize;

        public RemoteSpecifications() { }

        public RemoteSpecifications(string osName, string osType, string osPack, string cpuName, int cpuL, int cpuP, double cpuSpeed, int bits)
        {
            OsName = osName;
            OsType = osType;
            OsPack = osPack;

            CpuName = cpuName;

            CpuLCores = cpuL;
            CpuPCores = cpuP;
            CpuSpeed = cpuSpeed;

            BitSize = bits;
        }

        public override void Read(BinaryReader reader)
        {
            OsName = reader.ReadString();
            OsType = reader.ReadString();
            OsPack = reader.ReadString();

            CpuName = reader.ReadString();

            CpuLCores = reader.ReadInt32();
            CpuPCores = reader.ReadInt32();

            CpuSpeed = reader.ReadDouble();

            BitSize = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(OsName);
            writer.Write(OsType);
            writer.Write(OsPack);

            writer.Write(CpuName);

            writer.Write(CpuLCores);
            writer.Write(CpuPCores);

            writer.Write(CpuSpeed);

            writer.Write(BitSize);
        }
    }
}