using System;

namespace helpers
{
    [LogSource("Console Utils")]
    public static class ConsoleUtils
    {
        public static bool TryReadInput<T>(out T inputValue)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                inputValue = default;
                return false;
            }

            var result = Convert.ChangeType(input, typeof(T));
            if (result is null || !(result is T t))
            {
                inputValue = default;
                return false;
            }

            inputValue = t;
            return true;
        }

        public static bool TryReadInput<T>(Func<string, T> parser, out T inputValue)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                inputValue = default;
                return false;
            }

            inputValue = parser(input);
            return true;
        }
    }
}