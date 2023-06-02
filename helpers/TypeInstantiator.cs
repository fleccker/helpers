using System;
using System.Collections.Generic;

namespace helpers
{
    public static class TypeInstantiator
    {
        private static readonly Dictionary<Type, TypeInstantiatorMap> _mappedTypes = new Dictionary<Type, TypeInstantiatorMap>();

        public static object GetInstance(Type type) => GetMap(type).GetInstance();
        public static bool IsMapped(Type type) => _mappedTypes.ContainsKey(type);

        public static TypeInstantiatorMap GetMap(Type type) => IsMapped(type) ? _mappedTypes[type] : Map(type);
        public static TypeInstantiatorMap Map(Type type)
        {
            var map = new TypeInstantiatorMap(type);
            _mappedTypes[type] = map;
            return map;
        }

        public static void Clear() => _mappedTypes.Clear();
    }
}