using helpers.Events;

using System;

namespace helpers.Configuration.Register
{
    public class RegisterConfigBase
    {
        public Action<object> SetValueDelegate { get; set; }
        public Func<object> GetValueDelegate { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public Type ValueType { get; set; }
        public object DefaultValue { get; set; } = null;

        public readonly EventProvider OnValueChanged = new EventProvider();
        public readonly EventProvider OnRegistered = new EventProvider();
        public readonly EventProvider OnUnregistered = new EventProvider();

        internal void SetValue(object value)
        {
            SetValueDelegate?.Invoke(value);
            OnValueChanged.Invoke(value);
        }

        internal object GetValue() => GetValueDelegate?.Invoke() ?? DefaultValue;
    }
}