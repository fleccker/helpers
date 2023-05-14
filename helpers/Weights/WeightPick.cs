using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Weights
{
    public static class WeightPick
    {
        public static bool ChooseBool(int trueChance)
            => TryChoose<bool>(
                new WeightItem<bool>(trueChance, true),
                new WeightItem<bool>(100 - trueChance, false),
                
                out var chosen) ? chosen.Value : false;

        public static bool TryChoose<T>(WeightItem<T> itemOne, WeightItem<T> itemTwo, out WeightItem<T> chosenItem)
        {
            if (itemOne.Weight >= 100)
                throw new ArgumentOutOfRangeException($"List weight sum does not match 100.");

            chosenItem = Pick(new WeightItem<T>[] { itemOne, itemTwo });
            return chosenItem != null;
        }

        public static T Choose<T>(T itemOne, int itemOneWeight, T itemTwo)
        {
            if (TryChoose(
                new WeightItem<T>(itemOneWeight, itemOne),
                new WeightItem<T>(100 - itemOneWeight, itemTwo), 
                
                out var res))
            {
                return res.Value;
            }

            return default;
        }

        public static WeightItem<T> Pick<T>(this IEnumerable<WeightItem<T>> values) 
        {
            if (TryPick(values, out var res))
                return res;

            return default;
        }

        public static T Pick<T>(this IEnumerable<T> values, Func<T, int> itemWeightPicker)
        {
            if (TryPick(values, itemWeightPicker, out var res))
                return res;

            return default;
        }

        public static bool TryPick<T>(this IEnumerable<WeightItem<T>> values, out WeightItem<T> result)
            => TryPick(values, x => x.Weight, out result);

        public static bool TryPick<T>(this IEnumerable<T> values, Func<T, int> itemWeightPicker, out T result)
        {
            if (values is null || !values.Any())
            {
                result = default;
                return false;
            }

            var totalWeight = values.Sum(x => itemWeightPicker(x));
            var choice = RandomGenerator.Int32(0, totalWeight);
            var sum = 0;

            if (totalWeight != 100)
                throw new ArgumentOutOfRangeException($"List weight sum does not match 100.");

            foreach (var item in values)
            {
                var itemWeight = itemWeightPicker(item);

                for (int i = sum; i < itemWeight + sum; i++)
                {
                    if (i >= choice)
                    {
                        result = item;
                        return true;
                    }
                }

                sum += itemWeight;
            }

            result = values.First();
            return true;
        }
    }
}
