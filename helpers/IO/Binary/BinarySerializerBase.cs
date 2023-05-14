using System;
using System.IO;

namespace helpers.IO.Binary
{
    public class BinarySerializerBase
    {
        public virtual Type[] AcceptedTypes { get; }

        public virtual void Serialize(object obj, BinaryWriter writer) { }
        public virtual object Deserialize(Type type, BinaryReader reader) { return null; }
    }
}