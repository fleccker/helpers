using Newtonsoft.Json;

using System.Net;
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

        [JsonProperty("m_SourceBytes")]
        public byte[] SourceBytes;

        [JsonIgnore]
        public IPEndPoint Source { get; }

        public TcpPayload(ushort peerId, ushort messageId, byte[] payload, EndPoint source)
        {
            PeerId = peerId;
            MessageId = messageId;
            Payload = payload;
            SourceBytes = Encoding.UTF8.GetBytes(source.ToString());
            Source = null;
        }

        [JsonConstructor]
        public TcpPayload(
            [JsonProperty("m_PeerId")] ushort peerId, 
            [JsonProperty("m_Id")] ushort messageId, 
            [JsonProperty("m_Data")] byte[] payload,
            [JsonProperty("m_SourceBytes")] byte[] source)
        {
            PeerId = peerId;
            MessageId = messageId;
            Payload = payload;
            SourceBytes = source;

            var split = Encoding.UTF8.GetString(source).Split(':');

            Source = new IPEndPoint(IPAddress.Parse(split[0]), int.Parse(split[1]));
        }

        public byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}