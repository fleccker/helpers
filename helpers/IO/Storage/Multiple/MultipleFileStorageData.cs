using helpers.Extensions;
using helpers.Network.Extensions.Data;

using System;
using System.Collections.Generic;
using System.IO;

namespace helpers.IO.Storage.Multiple
{
    public class MultipleFileStorageData
    {
        private readonly Dictionary<Type, List<object>> m_Data = new Dictionary<Type, List<object>>();

        public int TotalCount => m_Data.Count;

        public IReadOnlyList<object> All
        {
            get
            {
                var list = new List<object>();

                m_Data.ForEach(data => list.AddRange(data.Value));

                return list;
            }
        }

        public void Add(object obj)
        {
            if (obj is null)
                return;

            var type = obj.GetType();

            if (!m_Data.ContainsKey(type))
                m_Data.Add(type, new List<object>());

            m_Data[type].Add(obj);
        }

        public void Read(byte[] bytes)
        {
            m_Data.Clear();

            using (var mem = new MemoryStream(bytes))
            using (var read = new BinaryReader(mem))
            {
                var count = read.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    var type = read.ReadType();
                    var typeCount = read.ReadInt32();

                    m_Data[type] = new List<object>();

                    for (int y = 0; y < typeCount; y++)
                    {
                        var obj = read.ReadObject();

                        if (obj != null)
                        {
                            m_Data[type].Add(obj);
                        }
                    }
                }
            }
        }

        public void Save(out byte[] bytes)
        {
            using (var mem = new MemoryStream())
            using (var write = new BinaryWriter(mem))
            {
                write.Write(m_Data.Count);

                foreach (var pair in m_Data)
                {
                    write.Write(pair.Key);
                    write.Write(pair.Value.Count);

                    foreach (var value in pair.Value)
                    {
                        write.WriteObject(value);
                    }
                }

                bytes = mem.ToArray();
            }
        }

        public void Clear() => m_Data.Clear();
    }
}