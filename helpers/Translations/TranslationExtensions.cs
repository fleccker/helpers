using System;

namespace helpers.Translations
{
    public static class TranslationExtensions
    {
        public static ITranslationEntry WithParameter(this ITranslationEntry entry, string parameterName, string parameterType, string parameterDescription)
        {
            entry.Parameters.Add(new Tuple<string, string, string>($"${parameterName}", parameterType, parameterDescription));
            return entry;
        }
    }
}