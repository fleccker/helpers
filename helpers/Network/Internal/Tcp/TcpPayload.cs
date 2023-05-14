using Newtonsoft.Json;

using System.Text;

namespace helpers.Network.Internal.Tcp
{
    public struct TcpPayload
    {
        [JsonProperty("m_PeerId")]
        public ushort PeerId;

        [JsonProperty("m_Id")]
        public ushort MessageId;

        [JsonProperty("m_Data")]
        public byte[] Payload;

        public TcpPayload(ushort peerId, ushort messageId, byte[] payload)
        {
            PeerId = peerId;
            MessageId = messageId;

            Payload = payload;
        }

        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}