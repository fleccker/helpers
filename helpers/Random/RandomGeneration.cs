using helpers.Extensions;
using helpers.Random.Engines;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpers.Random
{
    public class RandomGeneration
    {
        private readonly IRandomGenerationEngine _randomGenerationEngine;

        public static RandomGeneration Default { get; } = new RandomGeneration(Singleton<DefaultGenerationEngine>.Instance);

        public RandomGeneration(IRandomGenerationEngine randomGenerationEngine) => _randomGenerationEngine = randomGenerationEngine;

        public virtual int GetRandom(int minValue, int maxValue) => _randomGenerationEngine.GetRandomValue(minValue, maxValue);
        public virtual float GetRandom(float minValue, float maxValue) => _randomGenerationEngine.GetRandomValue(minValue, maxValue);
        public virtual byte GetRandom(byte minValue, byte maxValue) => _randomGenerationEngine.GetRandomValue(minValue, maxValue);
        public virtual bool GetBool() => GetRandom(0, 1) == 1;

        public virtual string GetString(int length, Action<string> finishString = null)
        {
            var str = "";
            
            for (int i = 0; i < length; i++)
            {
                var rndByte = GetRandom(byte.MinValue, byte.MaxValue);
                var strByte = Encoding.UTF8.GetString(new byte[] { rndByte }, 0, 1);

                str += strByte;
            }

            if (finishString != null) finishString(str);
            return str;
        }

        public virtual IEnumerable<byte> GenerateBytes(byte minElementSize, byte maxElementSize, int size = byte.MaxValue, RandomListGenerationMode mode = RandomListGenerationMode.AllowMultiple)
        {
            var list = new List<byte>(size);

            for (int i = 0; i < size; i++)
            {
                if (mode is RandomListGenerationMode.AllowMultiple) list[i] = GetRandom(minElementSize, maxElementSize);
                else
                {
                    var nextByte = GetRandom(minElementSize, maxElementSize);
                    while (list.Contains(nextByte)) nextByte = GetRandom(minElementSize, maxElementSize);
                    list[i] = nextByte;
                }
            }

            return list;
        }

        public virtual IEnumerable<int> GenerateValues(int minElementSize, int maxElementSize, int size = byte.MaxValue, RandomListGenerationMode mode = RandomListGenerationMode.AllowMultiple)
        {
            var list = new List<int>(size);

            for (int i = 0; i < size; i++)
            {
                if (mode is RandomListGenerationMode.AllowMultiple) list[i] = GetRandom(minElementSize, maxElementSize);
                else
                {
                    var nextInt = GetRandom(minElementSize, maxElementSize);
                    while (list.Contains(nextInt)) nextInt = GetRandom(minElementSize, maxElementSize);
                    list[i] = nextInt;
                }
            }

            return list;
        }

        public virtual IEnumerable<byte> GenerateBytes(int size = byte.MaxValue, RandomListGenerationMode mode = RandomListGenerationMode.AllowMultiple) => GenerateBytes(byte.MinValue, byte.MaxValue, size, mode);
        public virtual IEnumerable<int> GenerateValues(int size = byte.MaxValue, RandomListGenerationMode mode = RandomListGenerationMode.AllowMultiple) => GenerateValues(byte.MinValue, byte.MaxValue, size, mode);

        public virtual void RandomOrder<TElement>(IList<TElement> collection) => collection.Shuffle();
        public virtual void RestoreOrder<TElement>(IList<TElement> collection, IEnumerable<int> order)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = collection[order.ElementAt(i)];
            }          
        }

        public virtual int[] RandomOrderSave<TElement>(IList<TElement> collection)
        {
            var originalOrder = new int[collection.Count];
            var newOrder = GenerateValues(0, collection.Count, collection.Count, RandomListGenerationMode.SingleOccurence).ToArray();

            for (int i = 0; i < collection.Count; i++)
            {
                originalOrder[i] = newOrder[i];
                collection[i] = collection[newOrder[i]];
            }

            return originalOrder;
        }
    }
}