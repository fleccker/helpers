namespace helpers.Random
{
    public interface IRandomGenerationEngine
    {
        int GetRandomValue(int minValue, int maxValue);
        float GetRandomValue(float minValue, float maxValue);
        byte GetRandomValue(byte minValue, byte maxValue);
    }
}