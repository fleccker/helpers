using helpers.Json;
using helpers.Network.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace helpers.Network.Extensions.Data
{
    public static class ReaderExtensions
    {
        private static HashSet<Type> m_KnownToNotBeSerializables = new HashSet<Type>();
        private static HashSet<Type> m_KnownSerializables = new HashSet<Type>();

        public static DateTime ReadDateTime(this BinaryReader reader) => DateTime.FromBinary(reader.ReadInt64());
        public static TimeSpan ReadTimeSpan(this BinaryReader reader) => TimeSpan.FromTicks(reader.ReadInt64());

        public static DataPack ReadPack(this BinaryReader reader)
        {
            var pack = new DataPack();
            pack.Read(reader);
            return pack;
        }

        public static Guid ReadGuid(this BinaryReader reader)
        {
            var guidBytesLength = reader.ReadInt32();
            var guidBytes = reader.ReadBytes(guidBytesLength);

            return new Guid(guidBytes);
        }

        public static Type ReadType(this BinaryReader reader)
        {
            var typeName = reader.ReadString();
            if (!Reflection.TryLoadType(typeName, out var type)) throw new TypeLoadException($"Failed to load type: {typeName}");
            return type;
        }

        public static List<object> ReadObjects(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length <= 0) return new List<object>();
            else
            {
                var data = new List<object>();

                for (int i = 0; i < length; i++)
                {
                    var instance = reader.ReadObject();
                    data.Add(instance);
                }

                return data;
            }
        }

        public static void ReadObjects(this BinaryReader reader, IList<object> destination)
        {
            var length = reader.ReadInt32();
            if (length <= 0) return;
            else
            {
                for (int i = 0; i < length; i++)
                {
                    var instance = reader.ReadObject();
                    destination.Add(instance);
                }
            }
        }
 
        public static object ReadObject(this BinaryReader reader)
        {
            var type = reader.ReadType();
            if (type.IsSerializable())
            {

                var instance = Reflection.Instantiate<ISerializable>(type);
                instance.Read(reader);
                return instance;
            }
            else
            {
                var jsonBytesLength = reader.ReadInt32();
                var jsonBytes = reader.ReadBytes(jsonBytesLength);
                var json = Encoding.ASCII.GetString(jsonBytes);
                var instance = JsonHelper.FromJson(json, type);

                return instance;
            }
        }

        public static bool IsSerializable(this Type type)
        {
            if (m_KnownToNotBeSerializables.Contains(type))
            {
                return false;
            }

            if (m_KnownSerializables.Contains(type))
            {
                return true;
            }

            if (Reflection.HasInterface<ISerializable>(type))
            {
                m_KnownSerializables.Add(type);
                return true;
            }
            else
            {
                m_KnownToNotBeSerializables.Add(type);
                return false;
            }
        }
    }
}
