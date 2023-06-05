using helpers.Extensions;
using helpers.Pooling.Exceptions;

using System;
using System.Collections.Concurrent;

namespace helpers.Pooling
{
    [LogSource("Pool")]
    public class Pool<T>
    {
        internal Action<T> _prepareToGet;
        internal Action<T> _prepareToStore;
        internal Func<T> _constructor;

        public Pool(Action<T> prepareGet = null, Action<T> prepareStore = null, Func<T> constructor = null)
        {
            _prepareToGet = prepareGet;
            _prepareToStore = prepareStore;
            _constructor = constructor;

            Queue = new ConcurrentQueue<T>();
        }

        public ConcurrentQueue<T> Queue { get; private set; }

        public PoolMode Mode { get; set; } = PoolMode.NewOnEmpty;

        public void Destroy()
        {
            Queue.Clear();
            Queue = null;
        }

        public void Reset()
        {
            Queue.Clear();
        }

        public void Push(T value)
        {
            _prepareToStore?.Invoke(value);
            Queue.Enqueue(value);
        }

        public T Get()
        {
            if (!Queue.TryDequeue(out var value))
            {
                if (Mode is PoolMode.ThrowOnEmpty) throw new PoolEmptyException(typeof(Pool<T>));
                else if (Mode is PoolMode.DefaultOnEmpty || _constructor is null) value = default;
                else value = _constructor.Invoke();
            }

            _prepareToGet?.Invoke(value);
            return value;
        }

        public bool TryGet(out T value)
        {
            if (Queue.TryDequeue(out value))
            {
                _prepareToGet?.Invoke(value);
                return true;
            }

            return false;
        }
    }
}