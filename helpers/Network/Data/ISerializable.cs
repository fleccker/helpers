using System.IO;

namespace helpers.Network.Data
{
    public interface ISerializable
    {
        void Read(BinaryReader reader);
        void Write(BinaryWriter writer);
    }
}