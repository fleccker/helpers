namespace helpers.Network.Internal
{
    public static class PeerId
    {
        public static ushort Current { get; private set; } = 0;

        public static ushort Next => Current++;
        public static ushort Previous => (ushort)(Current - 1);
    }
}
