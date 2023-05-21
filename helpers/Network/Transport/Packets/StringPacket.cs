namespace helpers.Network.Transport.Packets
{
    public struct StringPacket
    {
        public string[] Parameters { get; set; }

        public StringPacket(params string[] parameters)
        {
            Parameters = parameters;
        }
    }
}