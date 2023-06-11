using helpers.Extensions;
using helpers.Json;
using helpers.Network.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace helpers.Network.Extensions.Data
{
    public static class WriterExtensions
    {
        public static void Write(this BinaryWriter writer, DateTime dateTime) => writer.Write(dateTime.ToBinary());
        public static void Write(this BinaryWriter writer, TimeSpan timeSpan) => writer.Write(timeSpan.Ticks);
        public static void Write(this BinaryWriter writer, DataPack pack) => pack.Write(writer);
        public static void Write(this BinaryWriter writer, Guid guid)
        {
            var bytes = guid.ToByteArray();
            var byteLength = bytes.Length;

            writer.Write(byteLength);
            writer.Write(bytes); 
        }

        public static void Write(this BinaryWriter writer, Type type)
        {
            writer.Write(type.AssemblyQualifiedName);
        }

        public static void WriteObject(this BinaryWriter writer, object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            else
            {
                var type = obj.GetType();

                writer.Write(type);

                if (obj is ISerializable serializable)
                {
                    serializable.Write(writer);
                    return;
                }
                else
                {
                    var json = JsonHelper.ToJson(obj, JsonOptionsBuilder.NotIndented);
                    var data = Encoding.ASCII.GetBytes(json);

                    writer.Write(data.Length);
                    writer.Write(data);
                }
            }
        }

        public static void WriteObjects(this BinaryWriter writer, params object[] objects) => writer.WriteObjects(objects.ToList());
        public static void WriteObjects(this BinaryWriter writer, IEnumerable<object> objects)
        {
            writer.Write(objects.Count());
            objects.ForEach(writer.WriteObject);
        }
    }
}