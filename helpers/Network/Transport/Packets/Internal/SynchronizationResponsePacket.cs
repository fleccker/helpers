namespace helpers.Network.Transport.Packets.Internal
{
    public struct SynchronizationResponsePacket
    {
        public ushort PeerId { get; set; }

        public SynchronizationResponsePacket(ushort peerId) => PeerId = peerId;
    }
}