using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Services
{
    [LogSource("Service Collection")]
    public class ServiceCollection : IServiceCollection
    {
        private Dictionary<Type, IService> _services;

        public ServiceCollection()
        {
            _services = new Dictionary<Type, IService>();
        }

        public ServiceCollection(Dictionary<Type, IService> services)
        {
            _services = services;
        }

        public IReadOnlyList<IService> Services => _services.Values.ToList();

        public event Action<Type, IService> OnServiceAdded;
        public event Action<Type, IService> OnServiceRemoved;
        public event Action<Type, IService> OnServiceInstantiated;

        public IService AddService(Type type, params object[] initArgs)
        {
            var service = Reflection.Instantiate<IService>(type);

            _services[type] = service;

            service.Collection = this;
            service.Start(this, initArgs);

            OnServiceAdded?.Invoke(type, service);

            return service;
        }

        public IService AddService(IService service, params object[] initArgs)
        {
            var serviceType = service.GetType();

            _services[serviceType] = service;

            service.Collection = this;
            service.Start(this, initArgs);

            OnServiceAdded?.Invoke(serviceType, service);

            return service;
        }

        public T AddService<T>(params object[] initArgs) where T : IService
        {
            var serviceType = typeof(T);
            var service = Reflection.Instantiate<T>(serviceType);

            _services[serviceType] = service;

            service.Collection = this;
            service.Start(this, initArgs);

            OnServiceAdded?.Invoke(serviceType, service);

            return service;
        }

        public T AddService<T>(T service, params object[] initArgs) where T : IService
        {
            var serviceType = typeof(T);

            _services[serviceType] = service;

            service.Collection = this;
            service.Start(this, initArgs);

            OnServiceAdded?.Invoke(serviceType, service);

            return service;
        }

        public IService GetService(Type type)
        {
            if (_services.TryGetValue(type, out var service))
                return service;

            throw new KeyNotFoundException($"Service of type {type.FullName} was not present in this collection.");
        }

        public T GetService<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service)
                && service is T tService)
                return tService;

            throw new KeyNotFoundException($"Service of type {type.FullName} was not present in this collection.");
        }

        public bool HasService(Type type)
            => _services.ContainsKey(type);

        public bool HasService<T>() where T : IService
            => _services.ContainsKey(typeof(T));

        public IService InstantiateService(Type serviceType)
        {
            var service = Reflection.Instantiate<IService>(serviceType);

            OnServiceInstantiated?.Invoke(serviceType, service); 
            return service;
        }

        public T InstantiateService<T>() where T : IService
        {
            var service = Reflection.Instantiate<T>(typeof(T));

            OnServiceInstantiated?.Invoke(typeof(T), service);
            return service;
        }

        public bool RemoveService(Type type)
        {
            if (_services.TryGetValue(type, out var service))
            {
                service.Stop();
                service.Collection = null;
            }

            if (_services.Remove(type))
            {
                OnServiceRemoved?.Invoke(type, service);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveService<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                service.Stop();
                service.Collection = null;
            }

            if (_services.Remove(type))
            {
                OnServiceRemoved?.Invoke(type, service);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IServiceProvider ToProvider()
        {
            var collection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            foreach (var service in _services)
                collection.AddSingleton(service.Key, service.Value);

            return collection.BuildServiceProvider();
        }

        public bool TryGetService(Type type, out IService service)
        {
            if (HasService(type))
            {
                service = _services[type];
                return true;
            }

            service = null;
            return false;
        }

        public bool TryGetService<T>(out T service)
        {
            var type = typeof(T);

            if (HasService(type) && _services[type] is T tService)
            {
                service = tService;
                return true;
            }

            service = default;
            return false;
        }
    }
}
