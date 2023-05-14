using helpers.Extensions;

using System.Collections.Generic;

namespace helpers.Pooling.Pools
{
    public class DictionaryPool<TKey, TValue> : Pool<Dictionary<TKey, TValue>>
    {
        public DictionaryPool()
        {
            _prepareToGet = PrepareGet;
            _prepareToStore = PrepareStore;
            _constructor = Construct;
        }

        public static DictionaryPool<TKey, TValue> Pool { get; } = new DictionaryPool<TKey, TValue>();

        public Dictionary<TKey, TValue> Get(IDictionary<TKey, TValue> source)
        {
            if (!TryGet(out var list))
            {
                return new Dictionary<TKey, TValue>(source);
            }

            list.AddRange(source);

            return list;
        }

        private void PrepareGet(Dictionary<TKey, TValue> list)
        {
            if (list is null)
                return;

            list.Clear();
        }

        private void PrepareStore(Dictionary<TKey, TValue> list)
        {
            list.Clear();
        }

        private Dictionary<TKey, TValue> Construct()
        {
            return new Dictionary<TKey, TValue>();
        }
    }
}
