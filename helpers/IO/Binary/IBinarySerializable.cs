using System.IO;

namespace helpers.IO.Binary
{
    public interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}