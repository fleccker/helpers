using helpers.Pooling.Pools;
using helpers.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace helpers.Extensions
{
    [LogSource("Collection Extensions")]
    public static class CollectionExtensions
    {
        public static TEnumerable To<TEnumerable>(this IEnumerable values) where TEnumerable : IEnumerable
        {
            if (values is TEnumerable targetEnumerable) return targetEnumerable;
            return default;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            var dict = new Dictionary<TKey, TValue>();

            foreach (var pair in collection)
            {
                if (pair.Key is null) continue;
                if (dict.ContainsKey(pair.Key)) dict[pair.Key] = pair.Value;
                else dict.Add(pair.Key, pair.Value);
            }

            return dict;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
        {
            if (keySelector is null) throw new ArgumentNullException($"This method requires the key selector.");

            var dict = new Dictionary<TKey, TValue>();

            foreach (var value in values)
            {
                var key = keySelector(value);
                if (key is null) continue;

                dict[key] = value;
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

        public static bool TryGetFirst<T>(this IEnumerable<T> values, Func<T, bool> predicate, out T value)
        {
            if (predicate is null) throw new ArgumentNullException($"This method requires the predicate to be present!");

            foreach (var val in values)
            {
                if (predicate(val))
                {
                    value = val;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable values, Func<T, bool> predicate, out T value)
        {
            if (predicate is null) throw new ArgumentNullException($"This method requires the predicate to be present!");

            foreach (var val in values)
            {
                if (val is null) continue;
                if (!(val is T t)) continue;
                if (t is null) continue;
                if (!predicate(t)) continue;

                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable values, out T value)
        {
            foreach (var val in values)
            {
                if (val is null) continue;
                if (!(val is T t)) continue;
                if (t is null) continue;

                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public static List<T> Where<T>(this IEnumerable collection, bool addNull = false, Func<T, bool> predicate = null)
        {
            var list = new List<T>();

            foreach (var obj in collection)
            {
                if (obj is null)
                {
                    if (addNull) list.Add(default);
                    else continue;
                }

                if (!(obj is T t)) continue;
                if (t is null) continue;
                if (predicate != null && !predicate(t)) continue;

                list.Add(t);
            }

            return list;
        }

        public static List<TType> WhereNot<TFilter, TType>(this IEnumerable collection, bool addNull = false, Func<TType, bool> predicate = null)
        {
            var list = new List<TType>();

            foreach (var obj in collection)
            {
                if (obj is null)
                {
                    if (addNull) list.Add(default);
                    else continue;
                }

                if (obj is TFilter) continue;
                if (predicate != null && predicate((TType)obj)) continue;

                list.Add((TType)obj);
            }

            return list;
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
            if (source.Count() != target.Count()) return false;

            for (int i = 0; i < source.Count(); i++)
            {
                var item = source.ElementAt(i);
                var targetItem = target.ElementAt(i);

                if (item is object itemObj && targetItem is object targetObj)
                {
                    if (itemObj is null && targetObj is null) continue;
                }

                if (item.Equals(targetItem)) continue;
                else return false;
            }

            return true;
        }

        public static int Count(this IEnumerable collection)
        {
            var count = 0;
            var enumerator = collection.GetEnumerator();

            while (enumerator.MoveNext()) count++;

            return count;
        }

        public static object ElementAt(this IEnumerable collection, int index)
        {
            var curIndex = 0;
            var enumerator = collection.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (index == curIndex) return enumerator.Current;
                else continue;
            }

            return null;
        }

        public static bool Any(this IEnumerable collection) => collection.Count() > 0;
        public static bool Any<TType>(this IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item is null) continue;
                if (item is TType) return true;
            }

            return false;
        }

        public static void Shuffle<T>(this ICollection<T> source)
        {
            var copy = ListPool<T>.Pool.Get(source);
            var size = copy.Count;

            while (size > 1)
            {
                size--;

                var index = RandomGeneration.Default.GetRandom(0, size + 1);
                var value = copy.ElementAt<T>(index);

                copy[index] = copy[size];
                copy[size] = value;
            }

            source.Clear();

            foreach (var value in copy) source.Add(value);

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