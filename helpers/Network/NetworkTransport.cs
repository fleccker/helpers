using System.Collections.Generic;
using System.Linq;

using helpers.IO.Binary;

namespace helpers.Network
{
    public class NetworkTransport
    {
        private readonly List<byte[]> _savedData = new List<byte[]>();
        private readonly List<object> _readData = new List<object>();

        public IReadOnlyList<byte[]> SavedData => _savedData;
        public IReadOnlyList<object> ReadData => _readData;

        public NetworkTransport() { }
        public NetworkTransport(byte[] data) => _readData = BinarySerialization.Deserialize<List<object>>(data);

        public void Write(object obj) => _savedData.Add(BinarySerialization.Serialize(obj));

        internal void ClearSaved() => _savedData.Clear();
        internal void ClearRead() => _readData.Clear();
        internal byte[] Combine() => BinarySerialization.Serialize(_savedData);
    }
}