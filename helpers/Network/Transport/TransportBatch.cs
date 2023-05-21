using helpers.IO.Binary;

using System;
using System.Collections.Generic;

namespace helpers.Network.Transport
{
    public struct TransportBatch
    {
        private Dictionary<string, byte[]> _dataByType;
        private Dictionary<string, Tuple<string, byte[]>> _dataByKey;

        public IReadOnlyDictionary<string, byte[]> DataByType { get => _dataByType; }
        public IReadOnlyDictionary<string, Tuple<string, byte[]>> DataByKey { get => _dataByKey; }

        // params ignored
        public TransportBatch(object state = null)
        {
            _dataByType = new Dictionary<string, byte[]>();
            _dataByKey = new Dictionary<string, Tuple<string, byte[]>>();
        }

        public T GetData<T>() => TryGetData<T>(out var data) ? data : default;
        public object GetData(string key) => TryGetData(key, out var data) ? data : default;

        public bool Any<T>() => TryGetData<T>(out _);
        public bool Any(string key) => _dataByKey.ContainsKey(key);
            
        public bool TryGetData<T>(out T result)
        {
            if (_dataByType.TryGetValue(typeof(T).AssemblyQualifiedName, out var data))
            {
                result = BinarySerialization.Deserialize<T>(data);
                return true;
            }

            result = default; 
            return false;
        }

        public bool TryGetData(string key, out object result)
        {
            if (_dataByKey.TryGetValue(key, out var dataTuple))
            {
                result = BinarySerialization.Deserialize(Type.GetType(dataTuple.Item1), dataTuple.Item2);
                return true;
            }

            result = default; 
            return false;
        }

        public TransportBatch WithData(object data)
        {
            var dataType = data.GetType();
            var dataBytes = BinarySerialization.Serialize(data);

            _dataByType[dataType.AssemblyQualifiedName] = dataBytes;
            return this;
        }

        public TransportBatch WithData(string key, object data)
        {
            var dataType = data.GetType();
            var dataBytes = BinarySerialization.Serialize(data);

            _dataByKey[key] = new Tuple<string, byte[]>(dataType.AssemblyQualifiedName, dataBytes);
            return this;
        }

        public TransportBatch RemoveData<T>()
        {
            _dataByType.Remove(typeof(T).AssemblyQualifiedName);
            return this;
        }

        public TransportBatch RemoveData(string key)
        {
            _dataByKey.Remove(key);
            return this;
        }

        public TransportBatch ClearData()
        {
            _dataByType.Clear();
            _dataByKey.Clear();
            return this;
        }

        public byte[] ToBytes() => 
            BinarySerialization.Serialize(new Tuple<byte[], byte[]>(
                BinarySerialization.Serialize(_dataByKey),
                BinarySerialization.Serialize(_dataByType)));

        public ArraySegment<byte> ToSegment() => new ArraySegment<byte>(ToBytes());

        public TransportBatch FromBytes(byte[] bytes)
        {
            var dataTuple = BinarySerialization.Deserialize<Tuple<byte[], byte[]>>(bytes);

            _dataByKey = BinarySerialization.Deserialize<Dictionary<string, Tuple<string, byte[]>>>(dataTuple.Item1);
            _dataByType = BinarySerialization.Deserialize<Dictionary<string, byte[]>>(dataTuple.Item2);

            return this;
        }
    }
}