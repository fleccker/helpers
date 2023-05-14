namespace helpers.Services
{
    public class ServiceBase : IService
    {
        public IServiceCollection Collection { get; set; }

        public virtual void Start(IServiceCollection serviceCollection, params object[] initArgs)
        {

        }

        public virtual void Stop()
        {

        }
    }
}