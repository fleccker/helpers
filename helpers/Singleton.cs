using System;
using System.Collections.Generic;

namespace helpers
{
    public static class Singleton<TType>
    {
        private static readonly Dictionary<Type, object> _instantiatedClasses = new Dictionary<Type, object>();

        public static TType Instance
        {
            get
            {
                if (!_instantiatedClasses.ContainsKey(typeof(TType))) Build();
                return _instantiatedClasses[typeof(TType)].As<TType>();
            }
            set => _instantiatedClasses[typeof(TType)] = value;
        }

        public static bool HasInstance => _instantiatedClasses.ContainsKey(typeof(TType));

        public static void Build()
        {
            var instance = Reflection.Instantiate<TType>();
            if (instance is null) throw new Exception($"Failed to create an instance of type {typeof(TType).FullName}!");
            else _instantiatedClasses[typeof(TType)] = instance;
        }

        public static void Dispose()
        {
            if (Instance is IDisposable disposable)
            {
                disposable.Dispose();
                _instantiatedClasses.Remove(typeof(TType));
            }
        }
    }
}