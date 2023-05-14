using System;
using System.Collections.Generic;

namespace helpers.Services
{
    public interface IServiceCollection
    {
        event Action<Type, IService> OnServiceAdded;
        event Action<Type, IService> OnServiceRemoved;
        event Action<Type, IService> OnServiceInstantiated;

        IReadOnlyList<IService> Services { get; }

        bool RemoveService(Type type);
        bool RemoveService<T>() where T : IService;

        bool HasService(Type type);
        bool HasService<T>() where T : IService;

        bool TryGetService(Type type, out IService service);
        bool TryGetService<T>(out T service);

        IService AddService(Type type, params object[] initArgs);
        IService AddService(IService service, params object[] initArgs);
        IService GetService(Type type);
        IService InstantiateService(Type serviceType);

        T AddService<T>(params object[] initArgs) where T : IService;
        T AddService<T>(T service, params object[] initArgs) where T : IService;
        T GetService<T>() where T : IService;
        T InstantiateService<T>() where T : IService;

        IServiceProvider ToProvider();
    }
}
