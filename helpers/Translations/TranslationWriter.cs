using helpers.Extensions;

using System.Linq;
using System.Text;

namespace helpers.Translations
{
    public class TranslationWriter
    {
        private StringBuilder m_Buffer;

        public void WriteLanguage(string languageName, string languageCode)
        {
            m_Buffer ??= new StringBuilder();
            m_Buffer.AppendLine($"Language: {languageName} ({languageCode})");
        }

        public void Write(ITranslationEntry entry) 
        {
            m_Buffer ??= new StringBuilder();
            m_Buffer.AppendLine();
            m_Buffer.AppendLine($"<-- {entry.Id} -->");

            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                m_Buffer.AppendLine();
                m_Buffer.AppendLine("## Description ##");
                m_Buffer.AppendLine($"# {entry.Description}");
                m_Buffer.AppendLine("## Description ##");
            }

            if (entry.Parameters != null && entry.Parameters.Any())
            {
                m_Buffer.AppendLine();
                m_Buffer.AppendLine($"## Parameters ##");

                for (int i = 0; i < entry.Parameters.Count; i++)
                {
                    var paramPair = entry.Parameters.ElementAt(i);

                    if (!string.IsNullOrWhiteSpace(paramPair.Item1) && !string.IsNullOrWhiteSpace(paramPair.Item2))
                    {
                        if (!string.IsNullOrWhiteSpace(paramPair.Item2))
                        {
                            m_Buffer.AppendLine($"[{i + 1}] {paramPair.Item1} ({paramPair.Item2}) # {paramPair.Item3}");
                        }
                        else
                        {
                            m_Buffer.AppendLine($"[{i + 1}] {paramPair.Item1} ({paramPair.Item2})");
                        }
                    }
                }

                m_Buffer.AppendLine("## Parameters ##");
            }

            m_Buffer.AppendLine();
            m_Buffer.AppendLine("## Translation ##");

            var lines = entry.StringValue.SplitLines();

            lines.ForEach(str => m_Buffer.AppendLine(str));

            m_Buffer.AppendLine("## Translation ##");
            m_Buffer.AppendLine();
            m_Buffer.AppendLine($">-- {entry.Id} --<");
        }

        public override string ToString()
        {
            var str = m_Buffer.ToString();
            m_Buffer.Clear();
            return str;
        }
    }
}