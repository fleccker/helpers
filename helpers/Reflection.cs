using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using helpers.Extensions;

using FastGenericNew;

using MonoMod.Utils;

namespace helpers
{
    [LogSource("Reflection")]
    public static class Reflection
    {
        public static readonly IReadOnlyList<TypeCode> PrimitiveTypes = new List<TypeCode>() 
                                                                        { TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16,
                                                                          TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64,
                                                                          TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal,
                                                                          TypeCode.DateTime, TypeCode.Char, TypeCode.String };

        public static Type[] GetGenericParameterConstraints(MethodInfo method)
        {
            var list = new List<Type>();
            method = method.GetGenericMethodDefinition();
            foreach (var arg in method.GetGenericArguments()) list.AddRange(arg.GetGenericParameterConstraints());
            return list.ToArray();
        }

        public static bool IsConstraint(MethodInfo method, GenericParameterAttributes genericParameterAttributes)
        {
            method = method.MakeGenericMethod();

            foreach (var arg in method.GetGenericArguments())
            {
                if (arg.GenericParameterAttributes == genericParameterAttributes || arg.GenericParameterAttributes.Has(genericParameterAttributes))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetField(Type type, string fieldName, object value, object handle = null)
            => TrySetField(type, fieldName, value, handle);

        public static void SetField<T>(string fieldName, object value, T handle = default)
            => SetField(typeof(T), fieldName, value, handle);

        public static bool TrySetField(Type type, string fieldName, object value, object handle = null)
        {
            try
            {
                var field = Field(type, fieldName);

                if (field is null)
                    return false;

                field.SetValue(value, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static TFieldValue GetField<TFieldValue>(Type type, string fieldName, object fieldHandle = null)
            => TryGetField<TFieldValue>(type, fieldName, fieldHandle, out var value) ? value : default;

        public static TFieldValue GetField<TFieldValue, TDeclaringType>(string fieldName, TDeclaringType fieldHandle = default)
            => TryGetField<TFieldValue>(typeof(TDeclaringType), fieldName, fieldHandle, out var value) ? value : default;

        public static bool TryGetField<TFieldValue>(Type type, string fieldName, object fieldHandle, out TFieldValue fieldValue)
        {
            fieldValue = default;

            try
            {
                var field = Field(type, fieldName);

                if (field is null)
                    return false;

                var value = field.GetValue(fieldHandle);

                if (!(value is TFieldValue valueCast))
                    return false;

                fieldValue = valueCast;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetProperty(Type type, string propertyName, object value, object handle = null)
            => TrySetProperty(type, propertyName, value, handle);

        public static void SetProperty<T>(string propertyName, object value, T handle = default)
            => TrySetProperty(typeof(T), propertyName, value, handle);

        public static bool TrySetProperty(Type type, string propertyName, object value, object handle = null)
        {
            var property = Property(type, propertyName);

            if (property is null)
                return false;

            try
            {
                property.SetValue(handle, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static TPropertyValue GetProperty<TPropertyValue>(Type type, string propertyName, object propertyHandle = null)
            => TryGetProperty<TPropertyValue>(type, propertyName, propertyHandle, out var value) ? value : default;

        public static TPropertyValue GetProperty<TPropertyValue, TDeclaringType>(string propertyName, TDeclaringType propertyHandle = default)
            => TryGetProperty<TPropertyValue>(typeof(TDeclaringType), propertyName, propertyHandle, out var value) ? value : default;

        public static bool TryGetProperty<TPropertyValue>(Type type, string propertyName, object propertyHandle, out TPropertyValue propertyValue)
        {
            propertyValue = default;

            try
            {
                var property = Property(type, propertyName);

                if (property is null)
                    return false;

                var value = property.GetValue(propertyHandle);

                if (!(value is TPropertyValue valueCast))
                    return false;

                propertyValue = valueCast;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static EventInfo Event(Type type, string eventName)
            => type.GetEvent(eventName);

        public static EventInfo Event(string typeName, string eventName)
            => Type(typeName)?.GetEvent(eventName);

        public static EventInfo Event<T>(string eventName)
            => Type<T>()?.GetEvent(eventName);

        public static PropertyInfo Property(Type type, string propertyName)
            => type.GetProperty(propertyName);

        public static PropertyInfo Property(string typeName, string propertyName)
            => Type(typeName)?.GetProperty(propertyName);

        public static PropertyInfo Property<T>(string propertyName)
            => Type<T>()?.GetProperty(propertyName);

        public static FieldInfo Field(Type type, string fieldName)
            => type.GetField(fieldName);

        public static FieldInfo Field(string typeName, string fieldName)
            => Type(typeName)?.GetField(fieldName);

        public static FieldInfo Field<T>(string fieldName)
            => Type<T>()?.GetField(fieldName);

        public static MethodInfo Method(string typeName, string methodName)
            => Type(typeName)?.GetMethod(methodName);

        public static MethodInfo Method(Type type, string methodName)
            => type.GetMethod(methodName);

        public static MethodInfo Method<T>(string methodName)
            => Method(typeof(T), methodName);

        public static Type Type(string typeName)
            => System.Type.GetType(typeName);

        public static Type Type(TypeCode typeCode)

        {
            switch (typeCode)
            {
                case TypeCode.Byte: return typeof(byte);
                case TypeCode.SByte: return typeof(sbyte);
                case TypeCode.Int16: return typeof(short);
                case TypeCode.UInt16: return typeof(ushort);
                case TypeCode.Int32: return typeof(int);
                case TypeCode.UInt32: return typeof(uint);
                case TypeCode.Int64: return typeof(long);
                case TypeCode.UInt64: return typeof(ulong);
                case TypeCode.Single: return typeof(float);
                case TypeCode.Double: return typeof(double);
                case TypeCode.Decimal: return typeof(decimal);
                case TypeCode.DateTime: return typeof(DateTime);
                case TypeCode.Char: return typeof(char);
                case TypeCode.String: return typeof(string);
                case TypeCode.Boolean: return typeof(bool);
                default: return null;
            }
        }

        public static Type Type<T>()
            => typeof(T);

        public static Assembly Assembly(string assemblyName)
        {
            var assemblies = GetLoadedAssemblies();

            return assemblies.FirstOrDefault(x => x.FullName.ToLower().Contains(assemblyName.ToLower()) || x.GetName().Name.ToLower().Contains(assemblyName.ToLower()));
        }

        public static void InvokeEvent(Type type, string eventName, object handle = null, params object[] eventArgs)
        {
            var evInfo = Event(type, eventName);

            evInfo.GetRaiseMethod(true)?.Invoke(handle, eventArgs);
        }

        public static void InvokeEvent<TDeclaringType>(string eventName, TDeclaringType handle = default, params object[] eventArgs)
        {
            InvokeEvent(typeof(TDeclaringType), eventName, handle, eventArgs);
        }

        public static bool TryInvokeEvent<TDeclaringType>(string eventName, TDeclaringType handle = default, params object[] eventArgs)
        {
            return TryInvokeEvent(typeof(TDeclaringType), eventName, handle, eventArgs);
        }

        public static bool TryInvokeEvent(Type type, string eventName, object handle = null, params object[] eventArgs)
        {
            try
            {
                InvokeEvent(type, eventName, handle, eventArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RemoveHandler(Type type, string eventName, Delegate handler, object handle = null)
        {
            var eventInfo = type.GetEvent(eventName);

            if (eventInfo is null)
            {
                throw new ArgumentException($"{type.FullName} does not contain event {eventName}");
            }

            eventInfo.RemoveEventHandler(handle, handler);
        }

        public static void RemoveHandler<T>(Type type, string eventName, T handler, object handle = null) where T : Delegate
        {
            RemoveHandler(type, eventName, handler, handle);
        }

        public static void RemoveHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener listener, TEventDeclaring handle = default) where TEventListener : Delegate
        {
            RemoveHandler(typeof(TEventDeclaring), eventName, listener, handle);
        }

        public static bool TryRemoveHandler(Type type, string eventName, Delegate listener, object handle = null)
        {
            try
            {
                RemoveHandler(type, eventName, listener, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryRemoveHandler<T>(Type type, string eventName, T listener, object handle = null) where T : Delegate
        {
            try
            {
                RemoveHandler<T>(type, eventName, listener, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryRemoveHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener listener, TEventDeclaring handle = default) where TEventListener : Delegate
        {
            try
            {
                RemoveHandler<TEventDeclaring, TEventListener>(eventName, listener, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void AddHandler<T>(Type type, string eventName, T action, object handle = null) where T : Delegate
        {
            var handlerType = typeof(T);
            var eventInfo = type.GetEvent(eventName);
            
            if (eventInfo is null)
            {
                throw new ArgumentException($"{type.FullName} does not contain event {eventName}");
            }

            if (handlerType != eventInfo.EventHandlerType)
            {
                throw new ArgumentException($"Type {handlerType.FullName} does not match event type {eventInfo.EventHandlerType.FullName}");
            }

            eventInfo.AddEventHandler(handle, action);
        }

        public static void AddHandler(Type eventDeclaringType, string eventName, Type methodDeclaringType, string methodName, object methodHandle = null, object eventHandle = null)
        {
            var eventInfo = eventDeclaringType.GetEvent(eventName);

            var delegateObj = methodHandle != null ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodHandle, methodName) : Delegate.CreateDelegate(eventInfo.EventHandlerType, methodDeclaringType.GetMethod(methodName));

            eventInfo.AddEventHandler(eventHandle, delegateObj);
        }

        public static void AddHandler(Type eventDeclaringType, string eventName, MethodInfo method, object methodHandle = null, object eventHandle = null)
        {
            var eventInfo = eventDeclaringType.GetEvent(eventName);

            var delegateObj = methodHandle != null ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodHandle, method.Name) : Delegate.CreateDelegate(eventInfo.EventHandlerType, method);

            eventInfo.AddEventHandler(eventHandle, delegateObj);
        }

        public static void AddHandler(Type eventDeclaringType, string eventName, Delegate listener, object eventHandle = null)
        {
            eventDeclaringType?.GetEvent(eventName)?.AddEventHandler(eventHandle, listener);   
        }

        public static void AddHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener listener, TEventDeclaring eventDeclaringHandle = default) where TEventListener : Delegate
        {
            AddHandler(typeof(TEventDeclaring), eventName, listener, eventDeclaringHandle);
        }

        public static bool TryAddHandler<T>(Type type, string eventName, T action, object handle = null) where T : Delegate
        {
            try
            {
                AddHandler(type, eventName, action, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryAddHandler(Type eventDeclaringType, string eventName, Delegate listener, object eventHandle = null)
        {
            try
            {
                AddHandler(eventDeclaringType, eventName, listener, eventHandle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryAddHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener eventListener, TEventDeclaring eventDeclaringHandle = default) where TEventListener : Delegate
        {
            try
            {
                AddHandler(eventName, eventListener, eventDeclaringHandle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static HashSet<Assembly> GetLoadedAssemblies()
        {
            var set = new HashSet<Assembly>();

            if (AppDomain.CurrentDomain != null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    set.Add(assembly);
                }
            }

            var callingAssembly = System.Reflection.Assembly.GetCallingAssembly();
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            if (!set.Contains(callingAssembly))
                set.Add(callingAssembly);

            if (!set.Contains(executingAssembly))
                set.Add(executingAssembly);

            return set;
        }

        public static T InstantiateWithGeneric<T>(Type genericType)
        {
            return As<T>(Instantiate(typeof(T).MakeGenericType(genericType)));
        }

        public static T InstantiateWithGeneric<T>(Type type, Type genericType)
        {
            return As<T>(Instantiate(type.MakeGenericType(genericType)));
        }

        public static T Instantiate<T>()
        {
            return FastNew.CreateInstance<T>();
        }

        public static T InstantiateAs<T>(Type type, params object[] args)
        {
            if (args is null
                || args.Any(x => x is null))
                throw new ArgumentNullException(nameof(args));

            var typeMap = args.Select(x => x.GetType()).ToArray();
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic);

            ConstructorInfo selectedConstructor = null;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var parameterTypes = parameters.Select(x => x.ParameterType);

                if (parameters.Length != typeMap.Length)
                    continue;

                if (!parameterTypes.Match(typeMap))
                    continue;

                selectedConstructor = constructor;
                break;
            }

            if (selectedConstructor is null)
                return default;

            var instance = selectedConstructor.Invoke(args);

            return As<T>(instance);
        }

        public static T InstantiateAs<T>(params object[] args)
        {
            if (args is null
                || args.Any(x => x is null))
                throw new ArgumentNullException(nameof(args));

            var type = typeof(T);
            var typeMap = args.Select(x => x.GetType()).ToArray();
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic);

            ConstructorInfo selectedConstructor = null;
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var parameterTypes = parameters.Select(x => x.ParameterType);

                if (parameters.Length != typeMap.Length)
                    continue;

                if (!parameterTypes.Match(typeMap))
                    continue;

                selectedConstructor = constructor;
                break;
            }

            if (selectedConstructor is null)
                return default;

            var instance = selectedConstructor.Invoke(args);

            return SafeAs<T>(instance);
        }

        public static void Execute(Type type, string methodName, object handle = null, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

            if (method is null)
                throw new MissingMethodException(methodName);

            method.Invoke(handle, parameters);
        }

        public static void Execute<T>(string methodName, T handle = default, params object[] parameters)
            => Execute(typeof(T), methodName, handle, parameters);

        public static T ExecuteReturn<T>(Type type, string methodName, object handle = null, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

            if (method is null)
                throw new MissingMethodException(methodName);

            var res = method.Invoke(handle, parameters);

            if (res is null)
                return default;

            if (!(res is T t))
                return default;

            return t;
        }

        public static TValue ExecuteReturn<TType, TValue>(string methodName, TType handle = default, params object[] parameters)
            => ExecuteReturn<TValue>(typeof(TType), methodName, handle, parameters);

        public static T Instantiate<T>(Type type)
        {
            return SafeAs<T>(Instantiate(type));
        }

        public static object Instantiate(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static T SafeAs<T>(this object obj)
        {
            if (!Is<T>(obj, out var value))
                return default;

            return value;
        }

        public static T As<T>(this object obj)
            => (T)obj;

        public static bool Is<T>(this object obj)
            => obj is T;

        public static bool Is<T>(this object obj, out T t)
        {
            if (obj is T value)
            {
                t = value;
                return true;
            }

            t = default;
            return false;
        }

        public static bool TryGetAttribute<T>(this MemberInfo member, out T attributeValue)
        {
            var attributes = member.GetCustomAttributes();

            if (!attributes.Any())
            {
                attributeValue = default;
                return false;
            }

            foreach (var attribute in attributes)
            {
                if (Is(attribute, out attributeValue))
                    return true;
            }

            attributeValue = default;
            return false;
        }

        public static T GetAttribute<T>(MemberInfo member)
        {
            if (!TryGetAttribute<T>(member, out var attribute))
                return default;

            return attribute;
        }

        public static bool HasInterface<T>(Type type, bool checkBaseForInterfaces = false)
            => HasInterface(typeof(T), type, checkBaseForInterfaces);

        public static bool HasInterface(Type interfaceType, Type type, bool checkBaseForInterfaces = false)
        {
            var interfaces = type.GetInterfaces();

            if (interfaces.Length > 0)
            {
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i] == interfaceType)
                        return true;

                    if (checkBaseForInterfaces)
                    {
                        var baseInterfaces = interfaces[i].GetInterfaces();

                        while (baseInterfaces != null && baseInterfaces.Length > 0)
                        {
                            for (int x = 0; x < baseInterfaces.Length; x++)
                            {
                                if (baseInterfaces[x] == interfaceType)
                                    return true;
                                else
                                    baseInterfaces = baseInterfaces[x].GetInterfaces();
                            }
                        }
                    }
                }
            }
            else if (type.BaseType != null && checkBaseForInterfaces)
            {
                return HasInterface(interfaceType, type.BaseType, checkBaseForInterfaces);
            }

            return false;
        }

        public static bool HasType<T>(Type searchType, bool checkBaseForInterfaces = false)
            => HasType(typeof(T), searchType, checkBaseForInterfaces);

        public static bool HasType(Type type, Type searchType, bool checkBaseForInterfaces = false)
        {
            if (type.BaseType != null)
            {
                if (type.BaseType == searchType)
                    return true;

                if (checkBaseForInterfaces)
                {
                    var baseType = type.BaseType;

                    while (baseType != null)
                    {
                        if (baseType == searchType)
                            return true;
                        else
                            baseType = baseType.BaseType;
                    }
                }
            }

            return false;
        }

        public static bool IsPrimitive(this Type type)
            => PrimitiveTypes.Contains(System.Type.GetTypeCode(type));

        public static bool IsEnumerable(this Type type)
            => HasInterface(type, typeof(IEnumerable), true);

        public static bool IsDictionary(this Type type)
            => HasInterface(type, typeof(IDictionary), true);
    }
}