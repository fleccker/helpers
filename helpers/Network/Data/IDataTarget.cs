namespace helpers.Network.Data
{
    public interface IDataTarget
    {
        bool Accepts(object data);
        bool Process(object data);
    }
}