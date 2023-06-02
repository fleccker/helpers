using helpers.Extensions;
using helpers.Random.Engines;

using System;
using System.Linq;

namespace helpers.Random
{
    public class WeightedRandomGeneration 
    {
        private readonly IRandomGenerationEngine _randomGenerationEngine;

        public static WeightedRandomGeneration Default { get; } = new WeightedRandomGeneration(Singleton<DefaultGenerationEngine>.Instance);

        public bool EnsureCorrectSum { get; set; } = true;

        public WeightedRandomGeneration(IRandomGenerationEngine randomGenerationEngine) => _randomGenerationEngine = randomGenerationEngine;

        public bool GetBool(int trueChance = 50) => PickObject<bool>(x =>
        {
            if (x is true) return trueChance;
            else return 100 - trueChance;
        }, true, false);

        public TObject PickObject<TObject>(Func<TObject, int> weightPicker, params TObject[] objects)
        {
            if (objects is null) throw new ArgumentNullException($"Parameter \"objects\" cannot be null or empty!");
            var objs = objects.ToList();
            if (!objs.Any()) throw new InvalidOperationException($"Cannot pick from an empty list!");
            var totalWeight = objs.Sum(weightPicker);
            if (totalWeight != 100 && EnsureCorrectSum) throw new InvalidOperationException($"The sum of the provided list is not equal to a hundred.");

            var choice = _randomGenerationEngine.GetRandomValue(0, totalWeight);
            var sum = 0;

            foreach (var item in objs)
            {
                var itemWeight = weightPicker(item);

                for (int i = sum; i < itemWeight + sum; i++)
                {
                    if (i >= choice)
                    {
                        return item;
                    }
                }

                sum += itemWeight;
            }

            return objs.First();
        }
    }
}