using System;
using System.Reflection;

namespace helpers.Analyzors.Instantiation
{
    public class TypeInstanceProperty
    {
        private Func<object> m_Getter;
        private Action<object> m_Setter;

        public PropertyInfo Property { get; }

        public object Value { get => Get(); set => Set(value); }

        public TypeInstanceProperty(PropertyInfo property)
        {
            Property = property;

            m_Getter = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), property.GetMethod);
            m_Setter = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), property.SetMethod);
        }

        public object Get() => m_Getter?.Invoke();
        public void Set(object value) => m_Setter.Invoke(value);
    }
}