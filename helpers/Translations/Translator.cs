namespace helpers.Translations
{
    public static class Translator
    {
        private static Translation m_Current;

        public static Translation Current => m_Current;

        public static void Set(string path, string languageName, string languageCode)
        {
            m_Current = new Translation(path, languageName, languageCode);
        }

        public static void Load()
        {
            if (m_Current is null)
                return;

            m_Current.Load();
        }

        public static void Save()
        {
            if (m_Current is null)
                return;

            m_Current.Save();
        }

        public static string Get(string id, params object[] parameters)
        {
            if (m_Current is null)
                return null;

            return m_Current.Get(id, parameters);
        }

        public static ITranslationEntry Add(string id, string value, string description)
        {
            if (m_Current is null)
                return null;

            return m_Current.Add(id, value, description);
        }
    }
}