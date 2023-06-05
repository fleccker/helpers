using helpers.Extensions;
using helpers.Random;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helpers.Network.Authentification.Storage
{
    public class KeyFileStorage : IAuthentificationStorage
    {
        private readonly HashSet<IAuthentificationData> _loaded = new HashSet<IAuthentificationData>();

        public string Path { get; set; }

        public KeyFileStorage(string path)
        {
            Path = path;
        }

        public bool IsValid(IAuthentificationData data) => _loaded.Any(x => x.ClientKey == data.ClientKey);

        public void SetTarget(object target)
        {
            Path = target.ToString();
            Reload();
        }

        public IAuthentificationData New()
        {
            var data = new AuthentificationData(RandomGeneration.Default.GetReadableString(30));
            _loaded.Add(data);
            Save();
            return data;
        }

        public IAuthentificationData Get(string key)
        {
            if (!_loaded.TryGetFirst(x => x.ClientKey == key, out var data)) return null;
            return data;
        }

        public void Remove(IAuthentificationData authentificationData)
        {
            if (_loaded.RemoveWhere(x => x.ClientKey == authentificationData.ClientKey) > 0)
                Save();
        }

        public void Reload()
        {
            if (!File.Exists(Path))
            {
                Save();
                return;
            }

            _loaded.Clear();

            var lines = File.ReadAllLines(Path);
            foreach (var line in lines) _loaded.Add(new AuthentificationData(line));
        }

        public void Save()
        {
            File.WriteAllLines(Path, _loaded.Select(x => x.ClientKey));
        }
    }
}