using helpers.Extensions;
using helpers.Translations.Entries;
using helpers.Translations.Exceptions;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helpers.Translations
{
    public class Translation
    {
        private HashSet<ITranslationEntry> m_Entries = new HashSet<ITranslationEntry>();

        internal string m_LanguageName;
        internal string m_LanguageCode;

        private string m_FilePath;

        private TranslationReader m_Reader;
        private TranslationWriter m_Writer;

        public Translation(string path, string languageName, string languageCode)
        {
            m_FilePath = path;
            m_LanguageName = languageName;
            m_LanguageCode = languageCode;

            m_Reader = new TranslationReader(this);
            m_Writer = new TranslationWriter();
        }

        public string LanguageName => m_LanguageName;
        public string LanguageCode => m_LanguageCode;

        public string FilePath => m_FilePath;
        public string FileName => Path.Combine(FilePath, $"translation.{LanguageName}");

        public IReadOnlyCollection<ITranslationEntry> Entries => m_Entries;

        public ITranslationEntry Add(string id, string value, string description)
        {
            if (TryGetEntry(id, out var entry))
            {
                entry.StringValue = value;
                return entry;
            }

            entry = new StringEntry(id, value, description);

            m_Entries.Add(entry);
            m_Entries = m_Entries.OrderByDescending(entry => entry.Id).ToHashSet();

            return entry;
        }

        public string Get(string id, params object[] parameters)
        {
            if (TryGetEntry(id, out var entry))
            {
                return entry.Translate(parameters.Select(parameter => parameter.ToString()).ToArray());
            }
            else
            {
                throw new TranslationEntryNotFoundException(id);
            }
        }

        public void Load()
        {
            if (!File.Exists(FileName))
            {
                Save();
                return;
            }

            var buffer = File.ReadAllLines(FileName);
            if (!buffer.Any())
                return;

            m_Reader.Read(buffer);
        }

        public void Save()
        {
            m_Writer.WriteLanguage(LanguageName, LanguageCode);
            m_Entries.ForEach(entry =>
            {
                m_Writer.Write(entry);
            });

            var str = m_Writer.ToString();

            File.WriteAllText(FileName, str);
        }

        public bool TryGetEntry(string entryId, out ITranslationEntry entry) => m_Entries.TryGetFirst(entry => entry.Id == entryId, out entry);
    }
}