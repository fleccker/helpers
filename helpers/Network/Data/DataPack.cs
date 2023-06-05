using helpers.Extensions;
using helpers.Network.Extensions.Data;

using System.Collections.Generic;
using System.IO;

namespace helpers.Network.Data
{
    public class DataPack
    { 
        private readonly HashSet<object> _pack = new HashSet<object>();

        public IReadOnlyCollection<object> Pack => _pack;

        public DataPack Write(params object[] objects)
        {
            _pack.AddRange(objects);
            return this;
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteObjects(_pack);
        }

        public void Read(BinaryReader reader)
        {
            Clear();
            _pack.AddRange(reader.ReadObjects());
        }

        public void Clear()
        {
            _pack.Clear();
        }
    }
}