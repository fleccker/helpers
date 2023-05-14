using helpers.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace helpers
{
    public static class RandomGenerator
    {
        private static Random _random = new Random();
        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static RandomNumberGenerator CryptoRandom { get => _rng; }
        public static Random Random { get => _random; }

        public static T Pick<T>(this IEnumerable<T> values)
        {
            return values.ElementAt(Int32(0, values.Count()));
        }

        public static int Int32(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static double Double()
        {
            return _random.NextDouble();
        }

        public static byte[] CryptoBytes(int size)
        {
            var bytes = new byte[size];
            _rng.GetBytes(bytes, 0, size);
            return bytes;
        }

        public static byte[] Bytes(int size)
        {
            var bytes = new byte[size];
            _random.NextBytes(bytes);
            return bytes;
        }

        public static byte[] Bytes()
        {
            return Bytes(Int32(byte.MinValue, byte.MaxValue));
        }

        public static byte[] CryptoBytes()
        {
            return CryptoBytes(Int32(byte.MinValue, byte.MaxValue));
        }

        public static byte[] CryptoNonZeroBytes(int size)
        {
            byte[] bytes = new byte[size];

            _rng.GetNonZeroBytes(bytes);

            return bytes;
        }

        public static int[] Order(int size)
        {
            var order = new int[size];

            for (int i = 0; i < order.Length; i++)
                order[i] = i;

            order.Shuffle();
            return order;
        }

        public static int[] Reorder(IList list)
        {
            var order = Order(list.Count);

            for (int i = 0; i < order.Length; i++)
            {
                list[i] = list[order[i]];
            }

            return order;
        }

        public static void Reorder(IList list, int[] order)
        {
            for (int i = 0; i < order.Length; i++)
            {
                list[i] = list[order[i]];
            }
        }

        public static void ReorderBack(IList list, int[] order)
        {
            for (int i = 0; i < order.Length; i++)
            {
                list[order[i]] = list[i];
            }
        }

        public static string String(int size)
        {
            return BitConverter.ToString(Bytes(size));
        }

        public static string CryptoString(int size)
        {
            return BitConverter.ToString(CryptoBytes(size));
        }

        public static string CryptoNonZeroString(int size)
        {
            return BitConverter.ToString(CryptoNonZeroBytes(size));
        }

        public static string String()
        {
            return BitConverter.ToString(Bytes());
        }

        public static string CryptoString()
        {
            return BitConverter.ToString(CryptoBytes());
        }

        public static string CryptoNonZeroString()
        {
            return BitConverter.ToString(CryptoNonZeroBytes(Int32(byte.MinValue, byte.MaxValue)));
        }

        public static string RandomIpV4Address()
        {
            return $"{Int32(1, 255)}.{Int32(1, 255)}.{Int32(1, 255)}.{Int32(1, 254)}";
        }

        public static string Ticket(int size)
        {
            return CryptoNonZeroString(size).Replace("-", "").ToLowerInvariant();
        }

        public static string GUID()
        {
            return Guid
                .NewGuid()
                .ToString()
                .ToLower();
        }

        public static bool Boolean()
        {
            return Int32(0, 1) == 1;
        }
    }
}