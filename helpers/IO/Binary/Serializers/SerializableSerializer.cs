using helpers;

using System;
using System.IO;

namespace helpers.IO.Binary.Serializers
{
    public class SerializableSerializer : BinarySerializerBase
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(IBinarySerializable) };

        public override object Deserialize(Type type, BinaryReader reader)
        {
            var serializable = Reflection.Instantiate<IBinarySerializable>(type);
            if (serializable is null)
                return null;

            serializable.Deserialize(reader);
            return serializable;
        }

        public override void Serialize(object obj, BinaryWriter writer)
        {
            if (obj is IBinarySerializable serializable)
                serializable.Serialize(writer);
        }
    }
}