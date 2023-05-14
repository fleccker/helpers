using helpers.Pooling.Pools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace helpers.Extensions
{
    public static class CollectionExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            var dict = new Dictionary<TKey, TValue>();

            foreach (var pair in collection)
            {
                if (dict.ContainsKey(pair.Key))
                    dict[pair.Key] = pair.Value;
                else
                    dict.Add(pair.Key, pair.Value); 
            }

            return dict;
        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out _))
                continue;
        }

        public static void EnqueueMany<T>(this ConcurrentQueue<T> queue, IEnumerable<T> source)
        {
            foreach (var item in source)
                queue.Enqueue(item);
        }

        public static void EnqueueMany<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (var item in source)
                queue.Enqueue(item);
        }

        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var value in values)
                action?.Invoke(value);
        }

        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action, Func<T, bool> match)
        {
            foreach (var value in values)
            {
                if (match(value))
                {
                    action(value);
                }
            }
        }

        public static bool TryPeekIndex<T>(this T[] array, int index, out T value)
        {
            if (index >= array.Length)
            {
                value = default;
                return false;
            }

            value = array[index];
            return true;
        }

        public static bool TryPeekIndex(this string str, int index, out char value)
        {
            if (index >= str.Length)
            {
                value = default;
                return false;
            }

            value = str[index];
            return true;
        }

        public static bool Match<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source.Count() != target.Count()) 
                return false;

            for (int i = 0; i < source.Count(); i++)
            {
                var item = source.ElementAt(i);
                var targetItem = target.ElementAt(i);
                
                if (item is object itemObj && targetItem is object targetObj)
                {
                    if (itemObj is null && targetObj is null) 
                        continue;
                }

                if (item.Equals(targetItem))
                    continue;
                else
                    return false;
            }

            return true;
        }    

        public static void Shuffle<T>(this ICollection<T> source)
        {
            var copy = ListPool<T>.Pool.Get(source);
            var size = copy.Count;

            while (size > 1)
            {
                size--;

                var index = RandomGenerator.Int32(0, size + 1);
                var value = copy.ElementAt(index);

                copy[index] = copy[size];
                copy[size] = value;
            }

            source.Clear();

            foreach (var value in copy)
                source.Add(value);

            ListPool<T>.Pool.Push(copy);
        }

        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source, Func<T, bool> condition)
        {
            foreach (var item in source)
            {
                if (!condition(item))
                    continue;

                destination.Add(item);
            }
        }

        public static bool TryDequeue<T>(this Queue<T> queue, out T value)
        {
            if (queue.Count <= 0)
            {
                value = default;
                return false;
            }

            value = queue.Dequeue();
            return true;
        }

        public static int FindIndex<T>(this T[] array, Func<T, bool> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }

        public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, out TKey key)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    return true;
                }
            }

            key = default;
            return false;
        }
    }
}