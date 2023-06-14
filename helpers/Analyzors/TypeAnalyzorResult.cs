using helpers.Analyzors.Instantiation;
using helpers.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace helpers.Analyzors
{
    public class TypeAnalyzorResult
    {
        private static readonly List<ITypeInstantiator> m_GlobalInstantiators = new List<ITypeInstantiator>();

        public static IReadOnlyList<ITypeInstantiator> GlobalInstantiators => m_GlobalInstantiators;

        private readonly List<Type> m_Interfaces = new List<Type>();
        private readonly List<Type> m_Types = new List<Type>();

        private List<ITypeInstantiator> m_Instantiators = new List<ITypeInstantiator>();

        private readonly List<FieldInfo> m_Fields = new List<FieldInfo>();
        private readonly List<PropertyInfo> m_Properties = new List<PropertyInfo>();
        private readonly List<ConstructorInfo> m_Constructors = new List<ConstructorInfo>();
        private readonly List<MethodInfo> m_Methods = new List<MethodInfo>();
        private readonly List<EventInfo> m_Events = new List<EventInfo>();

        private TypeInstanceProperty m_InstanceProperty;

        public Type Type { get; }

        public TypeInstanceProperty InstanceProperty => m_InstanceProperty;

        public object CachedInstance => m_InstanceProperty?.Value;

        public string TypeQualifiedName => Type.AssemblyQualifiedName;
        public string TypeName => Type.Name;
        public string TypeNamespace => Type.Namespace;

        public IReadOnlyList<ITypeInstantiator> Instantiators => m_Instantiators;

        public IReadOnlyList<Type> Interfaces => m_Interfaces;
        public IReadOnlyList<Type> Types => m_Types;

        public IReadOnlyList<FieldInfo> Fields => m_Fields;
        public IReadOnlyList<PropertyInfo> Properties => m_Properties;
        public IReadOnlyList<ConstructorInfo> Constructors => m_Constructors;
        public IReadOnlyList<MethodInfo> Methods => m_Methods;
        public IReadOnlyList<EventInfo> Events => m_Events;

        public TypeAnalyzorResult(Type type)
        {
            Type = type;
            Refresh();
        }

        public object GetInstance(object[] args = null, bool cached = true)
        {
            if (cached)
            {
                if (m_InstanceProperty != null)
                {
                    if (m_InstanceProperty.Value != null)
                        return m_InstanceProperty.Value;
                }
            }

            foreach (var instantiator in m_Instantiators)
            {
                var instance = instantiator.Instantiate(this, args);

                if (instance != null)
                {
                    if (cached)
                    {
                        if (m_InstanceProperty != null)
                            m_InstanceProperty.Value = instance;
                    }

                    return instance;
                }
            }

            return null;
        }

        public bool ImplementsInterface(Type type) => m_Interfaces.Contains(type);
        public bool ImplementsInterface<TInterface>() => ImplementsInterface(typeof(TInterface));

        public bool ImplementsType(Type type) => m_Types.Contains(type);
        public bool ImplementsType<TType>() => ImplementsType(typeof(TType));

        public void Refresh()
        {
            m_Instantiators.Clear();
            m_Interfaces.Clear();
            m_Types.Clear();
            m_Fields.Clear();
            m_Properties.Clear();

            ScanInterfaces();
            ScanTypes();
            ScanProperties();
            ScanFields();
            ScanConstructors();
            ScanMethods();
            ScanEvents();
            ScanInstantiators();
        }

        private void ScanInstantiators()
        {
            m_GlobalInstantiators.ForEach(instantiator =>
            {
                if (instantiator.IsAvailable(Type))
                    m_Instantiators.Add(instantiator);
            });

            m_Instantiators = m_Instantiators.OrderByDescending(inst => inst.Speed).ToList();
        }

        private void ScanInterfaces()
        {
            foreach (var interfaceType in Type.GetInterfaces())
            {
                m_Interfaces.Add(interfaceType);

                var baseInterfaces = interfaceType.GetInterfaces();

                baseInterfaces.ForEach(inter =>
                {
                    m_Interfaces.Add(inter);
                });
            }
        }

        private void ScanTypes()
        {
            var baseType = Type.BaseType;
            m_Types.Add(baseType);

            while (baseType != null)
            {
                baseType = baseType.BaseType;
                m_Types.Add(baseType);
            }
        }

        private void ScanProperties()
        {
            foreach (var property in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
            {
                m_Properties.Add(property);

                if (m_InstanceProperty is null)
                {
                    if (property.TryGetAttribute<TypeInstanceAttribute>(out _))
                    {
                        m_InstanceProperty = new TypeInstanceProperty(property);
                    }
                }
            }
        }

        private void ScanFields()
        {
            foreach (var field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
            {
                m_Fields.Add(field);
            }
        }

        private void ScanConstructors()
        {
            m_Constructors.AddRange(Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));
        }

        private void ScanMethods()
        {
            m_Methods.AddRange(Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));
        }

        private void ScanEvents()
        {
            m_Events.AddRange(Type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}