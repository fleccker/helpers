using helpers.Encryption;
using helpers.Json;
using System;
using System.IO;
using System.Text;

namespace helpers.IO.Binary.Serializers
{
    public class UnknownTypeSerializer : BinarySerializerBase
    {
        public override Type[] AcceptedTypes { get; } = Array.Empty<Type>();

        public override object Deserialize(Type type, BinaryReader reader)
        {
            var jsonStringEncrypted = reader.ReadBoolean();
            var jsonDataLength = reader.ReadInt32();
            var jsonData = Convert.FromBase64String(Encoding.ASCII.GetString(reader.ReadBytes(jsonDataLength)));
            var jsonString = "";

            if (jsonStringEncrypted)
            {
                jsonData = Encryptor.Decrypt(jsonData);
                jsonString = Encoding.ASCII.GetString(jsonData);
            }
            else
            {
                jsonString = Encoding.ASCII.GetString(jsonData);
            }

            return JsonHelper.FromJson(jsonString, type);
        }

        public override void Serialize(object obj, BinaryWriter writer)
        {
            var jsonString = JsonHelper.ToJson(obj);
            var bytes = Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(jsonString)));

            if (BinarySerialization.Encrypt)
            {
                bytes = Encryptor.Encrypt(bytes);

                writer.Write(true);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            else
            {
                writer.Write(false);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }
    }
}
