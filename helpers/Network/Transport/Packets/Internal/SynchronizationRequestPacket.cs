namespace helpers.Network.Transport.Packets.Internal
{
    public struct SynchronizationRequestPacket 
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public ushort Id { get; set; }

        public SynchronizationRequestPacket(string ip, int port, ushort id)
        {
            Ip = ip;
            Port = port;
            Id = id;
        }
    }
}