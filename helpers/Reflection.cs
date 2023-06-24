using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using helpers.Extensions;

using FastGenericNew;

using helpers.Analyzors;

namespace helpers
{
    [LogSource("Reflection")]
    public static class Reflection
    {
        public static IReadOnlyList<TypeCode> PrimitiveTypeCodes { get; } = new List<TypeCode>() 
                                                                        { TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16,
                                                                          TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64,
                                                                          TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal,
                                                                          TypeCode.DateTime, TypeCode.Char, TypeCode.String };

        public static IReadOnlyList<Type> PrimitiveTypes { get; } = PrimitiveTypeCodes.Select(x => Type(x)).ToList();

        public static MethodBase GetExecutingMethod(int skipFrames = 0) => new StackTrace().GetFrames().Skip(skipFrames + 1).First().GetMethod();

        public static bool TryLoadType(string typeName, out Type type)
        {
            type = System.Type.GetType(typeName);

            if (type is null)
            {
                foreach (var assembly in GetLoadedAssemblies())
                {
                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.FullName == typeName || t.Name == typeName)
                        {
                            type = t;
                            return true;
                        }
                    }
                }

                type = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        public static Action<TType> TypeProxy<TType, TProxy>(this Action<TProxy> toProxy, bool allowNull = false)
        {
            return x =>
            {
                if (x is null)
                {
                    if (allowNull) 
                        toProxy?.Invoke(default);
                    else 
                        return;

                    return;
                }

                if (!(x is TProxy t)) 
                    return;

                toProxy?.Invoke(t);
            };
        }

        public static Action<object> ObjectProxy<T>(this Action<T> toProxy, bool allowNull = true)
        {
            return x =>
            {
                if (x is null)
                {
                    if (allowNull)
                    {
                        toProxy?.Invoke(default);
                        return;
                    }
                    else
                        return;
                }

                if (!(x is T t)) 
                    return;

                toProxy?.Invoke(t);
            };
        }

        public static Func<object> ObjectProxy<T>(this Func<T> toProxy)
        {
            return () =>
            {
                var value = toProxy.Invoke();

                if (value is null) 
                    return null;
                else 
                    return value;
            };
        }

        public static Type[] GetGenericParameterConstraints(this MethodInfo method)
        {
            var list = new List<Type>();
            method = method.GetGenericMethodDefinition();

            foreach (var arg in method.GetGenericArguments()) 
                list.AddRange(arg.GetGenericParameterConstraints());

            return list.ToArray();
        }

        public static bool IsParam(this ParameterInfo parameter) => parameter.IsDefined(typeof(ParamArrayAttribute), false);
        public static bool IsConstraint(this MethodInfo method, GenericParameterAttributes genericParameterAttributes)
        {
            method = method.MakeGenericMethod();

            foreach (var arg in method.GetGenericArguments())
            {
                if (arg.GenericParameterAttributes == genericParameterAttributes || arg.GenericParameterAttributes.HasFlag(genericParameterAttributes))
                {
                    return true;
                }
            }

            return false;
        }

        public static void SetField(this Type type, string fieldName, object value, object handle = null) => TrySetField(type, fieldName, value, handle);
        public static void SetField<T>(string fieldName, object value, T handle = default) => SetField(typeof(T), fieldName, value, handle);
        public static bool TrySetField(this Type type, string fieldName, object value, object handle = null)
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

        public static TFieldValue GetField<TFieldValue>(this Type type, string fieldName, object fieldHandle = null) => TryGetField<TFieldValue>(type, fieldName, fieldHandle, out var value) ? value : default;
        public static TFieldValue GetField<TFieldValue, TDeclaringType>(string fieldName, TDeclaringType fieldHandle = default) => TryGetField<TFieldValue>(typeof(TDeclaringType), fieldName, fieldHandle, out var value) ? value : default;
        
        public static bool TryGetField<TFieldValue>(this Type type, string fieldName, object fieldHandle, out TFieldValue fieldValue)
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

        public static void SetProperty(this Type type, string propertyName, object value, object handle = null) => TrySetProperty(type, propertyName, value, handle);
        public static void SetProperty<T>(string propertyName, object value, T handle = default) => TrySetProperty(typeof(T), propertyName, value, handle);
        public static bool TrySetProperty(this Type type, string propertyName, object value, object handle = null)
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

        public static TPropertyValue GetProperty<TPropertyValue>(this Type type, string propertyName, object propertyHandle = null) => TryGetProperty<TPropertyValue>(type, propertyName, propertyHandle, out var value) ? value : default;
        public static TPropertyValue GetProperty<TPropertyValue, TDeclaringType>(string propertyName, TDeclaringType propertyHandle = default) => TryGetProperty<TPropertyValue>(typeof(TDeclaringType), propertyName, propertyHandle, out var value) ? value : default;
        
        public static bool TryGetProperty<TPropertyValue>(this Type type, string propertyName, object propertyHandle, out TPropertyValue propertyValue)
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

        public static EventInfo Event(this Type type, string eventName) => type.GetEvent(eventName);
        public static EventInfo Event(string typeName, string eventName) => Type(typeName)?.GetEvent(eventName);
        public static EventInfo Event<T>(string eventName) => Type<T>()?.GetEvent(eventName);

        public static PropertyInfo Property(this Type type, string propertyName) => type.GetProperty(propertyName);
        public static PropertyInfo Property(string typeName, string propertyName) => Type(typeName)?.GetProperty(propertyName);
        public static PropertyInfo Property<T>(string propertyName) => Type<T>()?.GetProperty(propertyName);

        public static FieldInfo Field(this Type type, string fieldName) => type.GetField(fieldName);
        public static FieldInfo Field(string typeName, string fieldName) => Type(typeName)?.GetField(fieldName);
        public static FieldInfo Field<T>(string fieldName) => Type<T>()?.GetField(fieldName);

        public static MethodInfo Method(string typeName, string methodName) => Type(typeName)?.GetMethod(methodName);
        public static MethodInfo Method(this Type type, string methodName) => type.GetMethod(methodName);
        public static MethodInfo Method<T>(string methodName) => Method(typeof(T), methodName);

        public static Type Type<T>() => typeof(T);
        public static Type Type(string typeName) => System.Type.GetType(typeName);
        public static Type Type(this TypeCode typeCode)

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

        public static Assembly Assembly(string assemblyName) => GetLoadedAssemblies().FirstOrDefault(x => x.FullName.ToLower().Contains(assemblyName.ToLower()) || x.GetName().Name.ToLower().Contains(assemblyName.ToLower()));

        public static void InvokeEvent(this Type type, string eventName, object handle = null, params object[] eventArgs) => Event(type, eventName).GetRaiseMethod(true)?.Invoke(handle, eventArgs);
        public static void InvokeEvent<TDeclaringType>(string eventName, TDeclaringType handle = default, params object[] eventArgs) => InvokeEvent(typeof(TDeclaringType), eventName, handle, eventArgs);

        public static bool TryInvokeEvent<TDeclaringType>(string eventName, TDeclaringType handle = default, params object[] eventArgs) => TryInvokeEvent(typeof(TDeclaringType), eventName, handle, eventArgs);
        public static bool TryInvokeEvent(this Type type, string eventName, object handle = null, params object[] eventArgs)
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

        public static void RemoveHandler(this Type type, string eventName, Delegate handler, object handle = null)
        {
            var eventInfo = type.GetEvent(eventName);

            if (eventInfo is null) 
                throw new ArgumentException($"{type.FullName} does not contain event {eventName}");

            eventInfo.RemoveEventHandler(handle, handler);
        }

        public static void RemoveHandler<T>(this Type type, string eventName, T handler, object handle = null) where T : Delegate => RemoveHandler(type, eventName, handler, handle);
        public static void RemoveHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener listener, TEventDeclaring handle = default) where TEventListener : Delegate => RemoveHandler(typeof(TEventDeclaring), eventName, listener, handle);

        public static bool TryRemoveHandler(this Type type, string eventName, Delegate listener, object handle = null)
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

        public static bool TryRemoveHandler<T>(this Type type, string eventName, T listener, object handle = null) where T : Delegate
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
                RemoveHandler(eventName, listener, handle);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void AddHandler<T>(this Type type, string eventName, T action, object handle = null) where T : Delegate
        {
            var handlerType = typeof(T);
            var eventInfo = type.GetEvent(eventName);
            
            if (eventInfo is null) 
                throw new ArgumentException($"{type.FullName} does not contain event {eventName}");
            
            if (handlerType != eventInfo.EventHandlerType) 
                throw new ArgumentException($"Type {handlerType.FullName} does not match event type {eventInfo.EventHandlerType.FullName}");

            eventInfo.AddEventHandler(handle, action);
        }

        public static void AddHandler(this Type eventDeclaringType, string eventName, Type methodDeclaringType, string methodName, object methodHandle = null, object eventHandle = null)
        {
            var eventInfo = eventDeclaringType.GetEvent(eventName);
            var delegateObj = methodHandle != null ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodHandle, methodName) : Delegate.CreateDelegate(eventInfo.EventHandlerType, methodDeclaringType.GetMethod(methodName));
            
            eventInfo.AddEventHandler(eventHandle, delegateObj);
        }

        public static void AddHandler(this Type eventDeclaringType, string eventName, MethodInfo method, object methodHandle = null, object eventHandle = null)
        {
            var eventInfo = eventDeclaringType.GetEvent(eventName);
            var delegateObj = methodHandle != null ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodHandle, method.Name) : Delegate.CreateDelegate(eventInfo.EventHandlerType, method);
            
            eventInfo.AddEventHandler(eventHandle, delegateObj);
        }

        public static void AddHandler(this Type eventDeclaringType, string eventName, Delegate listener, object eventHandle = null) => eventDeclaringType?.GetEvent(eventName)?.AddEventHandler(eventHandle, listener);
        public static void AddHandler<TEventDeclaring, TEventListener>(string eventName, TEventListener listener, TEventDeclaring eventDeclaringHandle = default) where TEventListener : Delegate => AddHandler(typeof(TEventDeclaring), eventName, listener, eventDeclaringHandle);

        public static bool TryAddHandler<T>(this Type type, string eventName, T action, object handle = null) where T : Delegate
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

        public static bool TryAddHandler(this Type eventDeclaringType, string eventName, Delegate listener, object eventHandle = null)
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

        public static T InstantiateWithGeneric<T>(Type genericType) => As<T>(Instantiate(typeof(T).MakeGenericType(genericType)));
        public static T InstantiateWithGeneric<T>(Type type, Type genericType) => As<T>(Instantiate(type.MakeGenericType(genericType)));
        public static T Instantiate<T>() => FastNew.CreateInstance<T>();
        public static T InstantiateAs<T>(Type type, params object[] args) => TypeAnalyzor.Analyze(type).GetInstance(args, false).As<T>();
        public static T InstantiateAs<T>(params object[] args) => TypeAnalyzor.Analyze(typeof(T)).GetInstance(args, false).As<T>();

        public static void Execute(Type type, string methodName, object handle = null, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

            if (method is null) 
                throw new MissingMethodException(methodName);

            method.Invoke(handle, parameters);
        }

        public static void Execute(Type type, string methodName, object handle, out object outValue, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

            if (method is null)
                throw new MissingMethodException(methodName);

            var paramList = new List<object>();
            var methodParams = method.GetParameters();

            var outAdded = false;
            var outIndex = methodParams.FindIndex(param => param.IsOut);

            if (outIndex < 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i == outIndex)
                    {
                        paramList.Add(null);
                        outAdded = true;
                    }
                    else
                    {
                        paramList.Add(parameters[i]);
                    }
                }
            }

            if (!outAdded)
            {
                paramList.Add(null);
                outAdded = true;
            }

            method.Invoke(handle, parameters);

            outValue = parameters[outIndex];
        }

        public static object ExecuteReturn(Type type, string methodName, object handle, out object outValue, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic);

            if (method is null)
                throw new MissingMethodException(methodName);

            var paramList = new List<object>();
            var methodParams = method.GetParameters();

            var outAdded = false;
            var outIndex = methodParams.FindIndex(param => param.IsOut);

            if (outIndex < 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i == outIndex)
                    {
                        paramList.Add(null);
                        outAdded = true;
                    }
                    else
                    {
                        paramList.Add(parameters[i]);
                    }
                }
            }

            if (!outAdded)
            {
                paramList.Add(null);
                outAdded = true;
            }

            var result = method.Invoke(handle, parameters);
            outValue = parameters[outIndex];
            return result;
        }

        public static void Execute<T>(string methodName, T handle = default, params object[] parameters) => Execute(typeof(T), methodName, handle, parameters);
        public static void Execute<T, TOut>(string methodName, T handle, out TOut outValue, params object[] parameters)
        {
            Execute(typeof(T), methodName, handle, out var result, parameters);

            if (result is null)
            {
                outValue = default;
                return;
            }

            if (!(result is TOut outV))
            {
                outValue = default;
                return;
            }

            outValue = outV;
        }

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

        public static TValue ExecuteReturn<TType, TValue>(string methodName, TType handle = default, params object[] parameters) => ExecuteReturn<TValue>(typeof(TType), methodName, handle, parameters);

        public static T Instantiate<T>(Type type) => SafeAs<T>(Instantiate(type));
        public static object Instantiate(Type type) => TypeAnalyzor.Analyze(type).GetInstance(null, false);

        public static T SafeAs<T>(this object obj)
        {
            if (!Is<T>(obj, out var value))  
                return default;

            return value;
        }
        public static T As<T>(this object obj) => (T)obj;
        public static bool Is<T>(this object obj) => obj is T;
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

        public static bool HasInterface<T>(Type type) => TypeAnalyzor.Analyze(type).ImplementsInterface<T>();
        public static bool HasInterface(Type interfaceType, Type type) => TypeAnalyzor.Analyze(type).ImplementsInterface(interfaceType);

        public static bool HasType<T>(Type searchType) => TypeAnalyzor.Analyze(searchType).ImplementsType<T>();
        public static bool HasType(Type type, Type searchType) => TypeAnalyzor.Analyze(searchType).ImplementsType(type);

        public static bool IsPrimitive(this Type type) => PrimitiveTypes.Contains(type);
        public static bool IsEnumerable(this Type type) => HasInterface(type, typeof(IEnumerable));
        public static bool IsDictionary(this Type type) => HasInterface(type, typeof(IDictionary));
    }
}