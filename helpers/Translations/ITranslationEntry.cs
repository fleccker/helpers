using System;
using System.Collections.Generic;

namespace helpers.Translations
{
    public interface ITranslationEntry
    {
        string StringValue { get; set; }
        string Description { get; }
        string Id { get; }

        List<Tuple<string, string, string>> Parameters { get; }

        void ReplaceParameters(ref string input, string[] parameters);

        string Translate(string[] parameters);
    }
}