using helpers.Extensions;

using System;
using System.IO;
using System.Linq;

namespace helpers.IO.Storage.Multiple
{
    public class MultipleFileStorage : IStorage<MultipleFileStorageData>
    {
        private MultipleFileStorageData m_Data;

        private readonly StorageMode m_Mode;
        private readonly string m_TargetPath;

        public int Size => m_Data != null ? m_Data.TotalCount : -1;
        public bool IsLoaded => m_Data != null;
        public string TargetPath => m_TargetPath;

        public MultipleFileStorageData Data => m_Data;

        public StorageMode Mode => m_Mode;

        public MultipleFileStorage(string path, StorageMode mode = StorageMode.Binary)
        {
            m_Mode = mode;
            m_TargetPath = path;

            if (mode != StorageMode.Binary)
            {
                throw new InvalidOperationException($"Multiple file storage does not support modes other than binary (yet).");
            }
        }

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

        public void Load()
        {
            m_Data ??= new MultipleFileStorageData();

            var bytes = File.ReadAllBytes(m_TargetPath);

            m_Data.Read(bytes);
        }

        public void Save()
        {
            if (m_Data is null)
                return;

            m_Data.Save(out var bytes);

            File.WriteAllBytes(m_TargetPath, bytes);
        }

        public object[] OfType(Type type) => m_Data.All.Where(data => data.GetType() == type).ToArray();
        public object OfTypeFirst(Type type) => m_Data.All.First(data => data.GetType() == type);

        public T[] OfType<T>() => m_Data.All.Where<T>().ToArray();
        public T OfTypeFirst<T>() => OfTypeFirst(typeof(T)).As<T>();

        public void Store(object obj) => (m_Data ??= new MultipleFileStorageData()).Add(obj);

        public void Clear() => m_Data?.Clear();
    }
}