using Fasterflect;

using helpers.Extensions;

using System;
using System.Reflection;

namespace helpers
{
    public struct TypeInstantiatorMap
    {
        public TypeInstantiatorMap(Type type)
        {
            Type = type;

            foreach (var field in type.GetFields())
            {
                if (field.IsDefined(typeof(InstanceAttribute), false)
                    && field.IsWritable()
                    && field.IsStatic
                    && field.FieldType == type)
                {
                    InstanceMember = type;
                    return;
                }
            }

            foreach (var property in type.GetProperties())
            {
                if (property.IsDefined(typeof(InstanceAttribute), false)
                    && property.CanWrite
                    && property.CanRead
                    && property.IsStatic()
                    && property.PropertyType == type)
                {
                    InstanceMember = property;
                    return;
                }
            }

            foreach (var method in type.GetMethods())
            {
                if (method.IsDefined(typeof(InstanceAttribute), false)
                    && method.IsStatic
                    && method.ReturnType == type)
                {
                    InstanceMember = method;
                    return;
                }
            }

            foreach (var constructor in type.GetConstructors())
            {
                if (constructor.IsDefined(typeof(InstanceAttribute), false)
                    && !constructor.GetParameters().Any())
                {
                    InstanceMember = constructor;
                    return;
                }
            }
        }

        public MemberInfo InstanceMember;
        public Type Type;
        public object Instance;

        public object GetInstance()
        {
            if (Instance is null) Construct();
            return Instance;
        }

        public Func<object> GetConstructor()
        {
            if (InstanceMember is null || Instance is null) return DefaultConstructor;
            else if (InstanceMember is FieldInfo) return FieldConstructor;
            else if (InstanceMember is PropertyInfo) return PropertyConstructor;
            else if (InstanceMember is MethodInfo) return MethodConstructor;
            else if (InstanceMember is ConstructorInfo) return Constructor;
            else return null;
        }

        public void Construct()
        {
            var constructor = GetConstructor();
            if (constructor is null) throw new InvalidOperationException($"Failed to construct {Type.FullName}: the constructor is null.");
            else Instance = constructor();
        }

        private object DefaultConstructor() => Activator.CreateInstance(Type);
        private object FieldConstructor()
        {
            var field = InstanceMember as FieldInfo;
            var value = field.GetValue(null);
            if (value is null)
            {
                var instance = DefaultConstructor();
                field.SetValue(null, instance);
                return instance;
            }
            else return value;
        }

        private object PropertyConstructor()
        {
            var property = InstanceMember as PropertyInfo;
            var value = property.GetValue(null);
            if (value is null)
            {
                var instance = DefaultConstructor();
                property.SetValue(null, instance);
                return instance;
            }
            else return value;
        }

        private object MethodConstructor()
        {
            var method = InstanceMember as MethodInfo;
            var value = method.Invoke(null, null);
            if (value is null) return DefaultConstructor();
            else return value;
        }
        
        private object Constructor()
        {
            var constructor = InstanceMember as ConstructorInfo;
            var value = constructor.Invoke(null);
            if (value is null) return DefaultConstructor();
            else return value;
        }
    }
}