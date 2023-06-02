using System;

namespace helpers.Random.Engines
{
    public class DefaultGenerationEngine : IRandomGenerationEngine
    {
        public static System.Random Random { get; } = new System.Random();

        public int GetRandomValue(int minValue, int maxValue) => Random.Next(minValue, maxValue);
        public float GetRandomValue(float minValue, float maxValue)
        {
            var mantissa = (Random.NextDouble() * 2.0) - 1.0;
            var exponent = Math.Pow(2.0, Random.Next(-126, 128));

            return (float)(mantissa * exponent);
        }

        public byte GetRandomValue(byte minValue, byte maxValue) => (byte)Random.Next(minValue, maxValue);
    }
}
