using System.Text;

using helpers.Translations.Exceptions;

namespace helpers.Translations
{
    public class TranslationReader
    {
        private Translation m_Translation;

        private string[] m_Buffer;

        private int m_Pos;

        public TranslationReader(Translation translation) => m_Translation = translation;

        public void Read(string[] buffer)
        {
            m_Buffer = buffer;

            string curId = null;

            ITranslationEntry entry = null;

            var isReading = false;
            var reader = new StringBuilder();

            while (!(m_Pos >= m_Buffer.Length))
            {
                var line = m_Buffer[m_Pos];

                m_Pos++;

                if (string.IsNullOrWhiteSpace(line) && !isReading)
                    continue;

                if (line.StartsWith("Language:") && !isReading)
                    continue;

                if (line.StartsWith("<--") && line.EndsWith("-->"))
                {
                    curId = line.Replace("<--", "").Replace("-->", "").Trim();
                    entry = null;

                    if (!m_Translation.TryGetEntry(curId, out entry))
                    {
                        throw new TranslationEntryNotFoundException(curId);
                    }
                }
                else if (line.StartsWith("##") && line.EndsWith("##"))
                {
                    if (!line.Contains("Translation"))
                        continue;

                    if (isReading)
                    {
                        isReading = false;
                        entry.StringValue = reader.ToString().Trim();
                        continue;
                    }

                    isReading = true;
                    reader.Clear();
                    continue;
                }
                else
                {
                    if (entry is null)
                        continue;

                    if (!isReading)
                        continue;

                    reader.AppendLine(line);
                }
            }
        }
    }
}