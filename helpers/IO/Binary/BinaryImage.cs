using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helpers.IO.Binary
{
    public class BinaryImage
    {
        private HashSet<object> _loadedObjects = new HashSet<object>();

        public void Store(object value) => TryStore(value);

        public T Retrieve<T>() => TryRetrieve<T>(out var result) ? result : default;

        public bool TryRetrieve<T>(out T result)
        {
            var res = _loadedObjects.FirstOrDefault(x => x is T);

            if (res is null)
            {
                result = default;
                return false;
            }

            result = (T)res;
            return true;
        }

        public bool TryStore(object value)
        {
            try
            {
                _loadedObjects.Add(value);
                return true;
            }
            catch { return false; }
        }

        public void Save(string path)
        {
            var combined = BinarySerialization.Serialize(_loadedObjects);

            if (combined is null)
                return;

            File.WriteAllBytes(path, combined);
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                Save(path);
                return;
            }

            var data = File.ReadAllBytes(path);

            _loadedObjects = BinarySerialization.Deserialize<HashSet<object>>(data);
        }

        public byte[] ToBytes()
        {
            return BinarySerialization.Serialize(_loadedObjects);
        }
    }
}