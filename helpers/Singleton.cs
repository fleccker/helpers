using System;
using System.Collections.Generic;

namespace helpers
{
    internal static class SingletonShared
    {
        internal static readonly Dictionary<Type, object> _instantiatedClasses = new Dictionary<Type, object>();
    }
    
    public static class Singleton
    {
        public static object Instance(Type type)
        {
            if (!SingletonShared._instantiatedClasses.ContainsKey(type)) Build(type);
            return SingletonShared._instantiatedClasses[type];
        }

        public static void Set(object instance) => SingletonShared._instantiatedClasses[instance.GetType()] = instance;
        public static void Build(Type type)
        {
            var instance = Reflection.Instantiate(type);
            if (instance is null) throw new Exception($"Failed to create an instance of type {type.FullName}!");
            else SingletonShared._instantiatedClasses[type] = instance;
        }

        public static void Dispose(Type type)
        {
            if (Instance(type) is IDisposable disposable)
            {
                disposable.Dispose();
                SingletonShared._instantiatedClasses.Remove(type);
            }
        }
    }

    public static class Singleton<TType>
    {
        public static TType Instance
        {
            get
            {
                if (!SingletonShared._instantiatedClasses.ContainsKey(typeof(TType))) Build();
                return SingletonShared._instantiatedClasses[typeof(TType)].As<TType>();
            }
            set => SingletonShared._instantiatedClasses[typeof(TType)] = value;
        }

        public static bool HasInstance => SingletonShared._instantiatedClasses.ContainsKey(typeof(TType));

        public static void Build()
        {
            var instance = Reflection.Instantiate<TType>();
            if (instance is null) throw new Exception($"Failed to create an instance of type {typeof(TType).FullName}!");
            else SingletonShared._instantiatedClasses[typeof(TType)] = instance;
        }

        public static void Dispose()
        {
            if (Instance is IDisposable disposable)
            {
                disposable.Dispose();
                SingletonShared._instantiatedClasses.Remove(typeof(TType));
            }
        }
    }
}