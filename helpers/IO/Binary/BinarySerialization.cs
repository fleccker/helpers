using helpers.IO.Binary.Serializers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace helpers.IO.Binary
{
    public static class BinarySerialization
    {
        private static HashSet<BinarySerializerBase> _serializers = new HashSet<BinarySerializerBase>();
        
        public static BinarySerializerBase MissingSerializer { get; set; }

        public static bool Encrypt { get; set; }


        public const string CombinedStringHeaderStart = "?*_combined";

        public static int CombinedStringHeaderSize { get => CombinedStringHeaderStart.Length; }

        static BinarySerialization()
        {
            SetSerializer(new SerializableSerializer());
            MissingSerializer = new UnknownTypeSerializer();
        }

        public static bool TryGetSerializer<T>(out BinarySerializerBase serializer) 
            => TryGetSerializer(typeof(T), out serializer);

        public static byte[] CombineAllSegments(params byte[][] otherBytes)
        {
            byte[] combined = null;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(CombinedStringHeaderStart);
                writer.Write(otherBytes.Length);

                foreach (var bytes in otherBytes)
                {
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }

                combined = stream.ToArray();
            }

            return combined;
        }

        public static byte[] CombineSegments(this byte[] bytesOne, params byte[][] otherBytes)
        {
            byte[] combined = null;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(CombinedStringHeaderStart);

                writer.Write(1 + otherBytes.Length);
                writer.Write(bytesOne.Length);
                writer.Write(bytesOne);

                foreach (var bytes in otherBytes)
                {
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }

                combined = stream.ToArray();
            }

            return combined;
        }

        public static bool IsCombined(byte[] bytes, out byte[][] otherBytes)
        {
            List<byte[]> otherList = new List<byte[]>();

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    var str = reader.ReadString();

                    if (string.IsNullOrWhiteSpace(str) || str != CombinedStringHeaderStart)
                    {
                        otherBytes = null;
                        return false;
                    }

                    var size = reader.ReadInt32();

                    for (int i = 0; i < size; i++)
                    {
                        otherList.Add(
                            reader.ReadBytes(
                                reader.ReadInt32()));
                    }
                }
                catch
                {
                    otherBytes = null;
                    return false;
                }
            }

            otherBytes = otherList.ToArray();
            return otherList.Count > 0;
        }

        public static bool TryGetSerializer(Type type, out BinarySerializerBase serializer)
        {
            if (Reflection.HasInterface<IBinarySerializable>(type, true))
                type = typeof(IBinarySerializable);
            else if (Reflection.IsDictionary(type))
                type = typeof(IDictionary);
            else if (Reflection.IsEnumerable(type))
                type = typeof(IEnumerable);

            serializer = _serializers.FirstOrDefault(x => x.AcceptedTypes.Contains(type));

            if (serializer is null)
                serializer = MissingSerializer;

            return serializer != null;
        }

        public static void SetSerializer(BinarySerializerBase serializer)
        {
            _serializers.Add(serializer);
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            object result = null;

            if (TryGetSerializer(type, out var serializer))
            {
                using (var memStream = new MemoryStream(bytes))
                using (var binaryReader = new BinaryReader(memStream))
                {
                    result = serializer.Deserialize(type, binaryReader);
                }
            }

            return result;
        }

        public static object Deserialize(Type type, BinaryReader reader)
        {
            object result = null;

            if (TryGetSerializer(type, out var serializer))
            {
                result = serializer.Deserialize(type, reader);
            }

            return result;
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            T result = default;

            if (TryGetSerializer<T>(out var serializer))
            {
                using (var memStream = new MemoryStream(bytes))
                using (var binaryReader = new BinaryReader(memStream))
                {
                    var res = serializer.Deserialize(typeof(T), binaryReader);              
                    if (res != null)
                    {
                        if (res is T t)
                        {
                            result = t;
                        }
                    }
                }
            }

            return result;
        }

        public static T Deserialize<T>(BinaryReader reader)
        {
            T result = default;

            if (TryGetSerializer<T>(out var serializer))
            {
                var res = serializer.Deserialize(typeof(T), reader);
                if (res != null)
                {
                    if (res is T t)
                    {
                        result = t;
                    }
                }
            }

            return result;
        }

        public static byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                return Array.Empty<byte>();

            if (TryGetSerializer<T>(out var serializer))
            {
                byte[] result = null;

                using (var memStream = new MemoryStream())
                using (var binaryWriter = new BinaryWriter(memStream))
                {
                    serializer.Serialize(obj, binaryWriter);
                    result = memStream.ToArray();
                }

                return result;
            }

            return null;
        }

        public static void Serialize<T>(T obj, BinaryWriter writer)
        {
            if (obj == null) 
                return;

            if (TryGetSerializer<T>(out var serializer))
            {
                serializer.Serialize(obj, writer);
            }
        }
    }
}
