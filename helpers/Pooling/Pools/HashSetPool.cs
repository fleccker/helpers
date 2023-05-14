using helpers.Extensions;

using System.Collections.Generic;

namespace helpers.Pooling.Pools
{
    public class HashSetPool<TElement> : Pool<HashSet<TElement>>
    {
        public HashSetPool()
        {
            _prepareToGet = PrepareGet;
            _prepareToStore = PrepareStore;
            _constructor = Construct;
        }

        public static HashSetPool<TElement> Pool { get; } = new HashSetPool<TElement>();

        public HashSet<TElement> Get(IEnumerable<TElement> source)
        {
            if (!TryGet(out var list))
            {
                return new HashSet<TElement>(source);
            }

            list.AddRange(source);

            return list;
        }

        private void PrepareGet(HashSet<TElement> list)
        {
            if (list is null)
                return;

            list.Clear();
        }

        private void PrepareStore(HashSet<TElement> list)
        {
            list.Clear();
        }

        private HashSet<TElement> Construct()
        {
            return new HashSet<TElement>();
        }
    }
}
