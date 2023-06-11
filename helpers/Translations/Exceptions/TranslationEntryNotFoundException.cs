using System;

namespace helpers.Translations.Exceptions
{
    public class TranslationEntryNotFoundException : Exception
    {
        public TranslationEntryNotFoundException(string entryId) : base($"Failed to find a translation entry with ID: {entryId}") { }
        public static void Throw(string entryId) => throw new TranslationEntryNotFoundException(entryId);
    }
}