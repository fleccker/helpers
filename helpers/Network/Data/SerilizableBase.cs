using System.IO;

namespace helpers.Network.Data
{
    public class SerilizableBase : ISerializable
    {
        public virtual void Read(BinaryReader reader)
        {

        }

        public virtual void Write(BinaryWriter writer)
        {

        }
    }
}