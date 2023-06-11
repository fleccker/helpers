using System;
using System.Collections.Generic;

namespace helpers.Translations.Entries
{
    public class StringEntry : ITranslationEntry
    {
        private string m_Value;
        private string m_Description;
        private string m_Id;

        private List<Tuple<string, string, string>> m_Params = new List<Tuple<string, string, string>>();

        public StringEntry(string id, string value, string description)
        {
            m_Id = id;
            m_Value = value;
            m_Description = description;
        }

        public string StringValue { get => m_Value; set => m_Value = value; }
        public string Description => m_Description;
        public string Id => m_Id;

        public List<Tuple<string, string, string>> Parameters => m_Params;

        public void ReplaceParameters(ref string input, string[] parameters)
        {
            if (parameters.Length != m_Params.Count)
            {
                throw new ArgumentException($"There are either too many or too few parameters! (Required: {m_Params.Count} / Received: {parameters.Length})");
            }

            for (int i = 0; i < m_Params.Count; i++)
            {
                var curParam = m_Params[i].Item1;
                input = input.Replace(curParam, parameters[i]);
            }
        }

        public string Translate(string[] parameters)
        {
            string input = new string(m_Value.ToCharArray());
            ReplaceParameters(ref input, parameters);
            return input;
        }
    }
}