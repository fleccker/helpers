using System.IO;

namespace helpers.Network.Authentification
{
    public struct AuthentificationData : IAuthentificationData
    {
        private string m_ClientKey;
        public string ClientKey => m_ClientKey;

        public AuthentificationData(string key) => m_ClientKey = key;

        public void Read(BinaryReader reader)
        {
            m_ClientKey = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(m_ClientKey);
        }
    }
}