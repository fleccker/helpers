using helpers.Configuration.Converters.Yaml;
using helpers.Extensions;
using helpers.IO.Binary;
using helpers.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace helpers.IO.Storage
{
    public class SingleFileStorage<TData> : IStorage<IReadOnlyCollection<TData>>
    {
        private readonly HashSet<TData> m_Data = new HashSet<TData>();
        private readonly string m_TargetPath;
        private readonly StorageMode m_Mode;

        private bool m_Loaded;

        public IReadOnlyCollection<TData> Data => m_Data;

        public StorageMode Mode => m_Mode;

        public int Size => m_Data.Count;
        public bool IsLoaded => m_Loaded;
        public string TargetPath => m_TargetPath;

        public SingleFileStorage(string targetPath, StorageMode mode = StorageMode.Binary)
        {
            m_TargetPath = targetPath;
            m_Mode = mode;
        }

        public bool Add(TData data) => m_Data.Add(data);
        public bool Remove(TData data) => m_Data.Remove(data);
        public bool Contains(TData data) => m_Data.Contains(data);

        public void ForEach(Action<TData> action) => m_Data.ForEach(action);

        public bool TryGet(Func<TData, bool> predicate, out TData data) => m_Data.TryGetFirst(predicate, out data);

        public void Reload()
        {
            if (!File.Exists(m_TargetPath))
            {
                Save();
                return;
            }

            Clear();
            Load();
        }

        public void Save()
        {
            switch (m_Mode)
            {
                case StorageMode.Binary:
                    {
                        var image = new BinaryImage();

                        image.Store(m_Data);
                        image.Save(m_TargetPath);

                        return;
                    }

                case StorageMode.Yaml:
                    {
                        var str = YamlParsers.Serializer.Serialize(m_Data);

                        File.WriteAllText(m_TargetPath, str);

                        return;
                    }

                case StorageMode.Json:
                    {
                        var str = JsonHelper.ToJson(m_Data, JsonOptionsBuilder.NotIndented);

                        File.WriteAllText(m_TargetPath, str);

                        return;
                    }

                case StorageMode.IndentedJson:
                    {
                        var str = JsonHelper.ToJson(m_Data, JsonOptionsBuilder.Indented);

                        File.WriteAllText(m_TargetPath, str);

                        return;
                    }
            }
        }

        public void Load()
        {
            switch (m_Mode)
            {
                case StorageMode.Binary:
                    {
                        var image = new BinaryImage();

                        image.Load(m_TargetPath);

                        if (!image.TryRetrieve<HashSet<TData>>(out var res))
                        {
                            Save();
                            return;
                        }

                        m_Data.UnionWith(res);
                        return;
                    }

                case StorageMode.Yaml:
                    {
                        var str = File.ReadAllText(m_TargetPath);
                        var obj = YamlParsers.Deserializer.Deserialize<HashSet<TData>>(str);

                        m_Data.UnionWith(obj);
                        return;
                    }

                case StorageMode.IndentedJson:
                case StorageMode.Json:
                    {
                        var str = File.ReadAllText(m_TargetPath);
                        var obj = JsonHelper.FromJson<HashSet<TData>>(str);

                        m_Data.UnionWith(obj);
                        return;
                    }
            }
        }

        public void Clear() => m_Data.Clear();
    }
}