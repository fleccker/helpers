using helpers.Configuration.Converters.Yaml;

using System;
using System.IO;

namespace helpers.IO.Binary.Serializers
{
    public class UnknownTypeSerializer : BinarySerializerBase
    {
        public override Type[] AcceptedTypes { get; } = Array.Empty<Type>();

        public override object Deserialize(Type type, BinaryReader reader) => YamlParsers.Deserializer.Deserialize(reader.ReadString(), type);
        public override void Serialize(object obj, BinaryWriter writer) => writer.Write(YamlParsers.Serializer.Serialize(obj));
    }
}