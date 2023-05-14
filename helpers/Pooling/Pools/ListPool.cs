using System.Collections.Generic;

namespace helpers.Pooling.Pools
{
    public class ListPool<TElement> : Pool<List<TElement>>
    {
        public ListPool() 
        {
            _prepareToGet = PrepareGet;
            _prepareToStore = PrepareStore;
            _constructor = Construct;
        }

        public static ListPool<TElement> Pool { get; } = new ListPool<TElement>();

        public List<TElement> Get(int size)
        {
            if (!TryGet(out var list))
            {
                return new List<TElement>(size);
            }

            list.Capacity = size;

            return list;
        }

        public List<TElement> Get(IEnumerable<TElement> source)
        {
            if (!TryGet(out var list))
            {
                return new List<TElement>(source);
            }

            list.AddRange(source);

            return list;
        }

        private void PrepareGet(List<TElement> list)
        {
            if (list is null)
                return;

            list.Clear();
        }

        private void PrepareStore(List<TElement> list)
        {
            list.Clear();
        }

        private List<TElement> Construct()
        {
            return new List<TElement>();
        }
    }
}
