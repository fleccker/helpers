namespace helpers.Services
{
    public interface IService
    {
        IServiceCollection Collection { get; set; }


        void Start(IServiceCollection serviceCollection, params object[] initArgs);
        void Stop();
    }
}