using helpers.Network.Data;

using System.IO;

namespace helpers.Network.Authentification.Messages
{
    public struct AuthentificationResponseMessage : ISerializable
    {
        private IAuthentificationData m_Data;

        public IAuthentificationData Data { get; }

        public AuthentificationResponseMessage(IAuthentificationData authentificationData)
        {
            m_Data = authentificationData;
        }

        public void Read(BinaryReader reader)
        {
            m_Data = new AuthentificationData(reader.ReadString());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(m_Data.ClientKey);
        }
    }
}