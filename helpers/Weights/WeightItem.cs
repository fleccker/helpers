namespace helpers.Weights
{
    public class WeightItem<T> 
    {
        public int Weight { get; }

        public T Value { get; }

        public WeightItem(int weight, T value)
        {
            Weight = weight;
            Value = value;
        }
    }
}