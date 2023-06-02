namespace helpers.Random
{
    public class WeightedObject
    {
        public WeightedObject(int weight, object obj)
        {
            Weight = weight;
            Object = obj;
        }

        public int Weight { get; set; }
        public object Object { get; }

        public static WeightedObject Create(object obj, int weight) => new WeightedObject(weight, obj);
    }
}