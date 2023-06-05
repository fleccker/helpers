using Fasterflect;

using System;
using System.ComponentModel;
using System.Reflection;

namespace helpers.Configuration.Register
{
    public static class RegisterConfigUtils
    {
        public static RegisterConfigBase Bind<T>(string configName, T defaultValue, Action<T> onValueChanged, Func<T> getValue, string description = null)
        {
            if (defaultValue is null) throw new ArgumentNullException(nameof(defaultValue));
            if (onValueChanged is null) throw new ArgumentNullException(nameof(onValueChanged));
            if (getValue is null) throw new ArgumentNullException(nameof(getValue));
            if (string.IsNullOrWhiteSpace(configName)) throw new ArgumentNullException(nameof(configName));

            return new RegisterConfigBase()
            {
                DefaultValue = defaultValue,
                Name = configName,
                GetValueDelegate = getValue.ObjectProxy(),
                SetValueDelegate = onValueChanged.ObjectProxy(),
                ValueType = typeof(T),
                Description = description
            };
        }

        public static RegisterConfigBase BindField(this object obj, string fieldName, string configName = null, object defaultValue = null, string description = null)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            var type = obj.GetType();
            var field = Reflection.Field(type, fieldName);
            if (field is null) throw new ArgumentException($"Field {fieldName} does not exist in type {type.FullName}");

            if (string.IsNullOrWhiteSpace(configName)) configName = fieldName;
            if (string.IsNullOrWhiteSpace(description) && field.TryGetAttribute<DescriptionAttribute>(out var descriptionAttribute)) description = descriptionAttribute.Description;

            return new RegisterConfigBase()
            {
                SetValueDelegate = x => field.Set(obj, x),
                GetValueDelegate = () => field.Get(obj),
                DefaultValue = defaultValue,
                Name = configName,
                ValueType = field.FieldType,
                Description = description
            };
        }

        public static RegisterConfigBase BindField(this Type type, string fieldName, string configName = null, object handle = null, object defaultValue = null, string description = null)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            var field = Reflection.Field(type, fieldName);
            if (field is null) throw new ArgumentException($"Field {fieldName} does not exist in type {type.FullName}");

            if (string.IsNullOrWhiteSpace(configName)) configName = fieldName;
            if (string.IsNullOrWhiteSpace(description) && field.TryGetAttribute<DescriptionAttribute>(out var descriptionAttribute)) description = descriptionAttribute.Description;

            return new RegisterConfigBase()
            {
                SetValueDelegate = x => SetFieldBind(field, handle, x),
                GetValueDelegate = () => GetFieldBind(field, handle),
                DefaultValue = defaultValue,
                Name = configName,
                ValueType = field.FieldType,
                Description = description
            };
        }

        public static RegisterConfigBase BindProperty(this object obj, string propertyName, string configName = null, object defaultValue = null, string description = null)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            var type = obj.GetType();
            var property = Reflection.Property(type, propertyName);
            if (property is null) throw new ArgumentException($"Property {propertyName} does not exist in type {type.FullName}");

            if (string.IsNullOrWhiteSpace(configName)) configName = propertyName;
            if (string.IsNullOrWhiteSpace(description) && property.TryGetAttribute<DescriptionAttribute>(out var descriptionAttribute)) description = descriptionAttribute.Description;

            return new RegisterConfigBase()
            {
                SetValueDelegate = x => property.Set(obj, x),
                GetValueDelegate = () => property.Get(obj),
                DefaultValue = defaultValue,
                Name = configName,
                ValueType = property.PropertyType,
                Description = description
            };
        }

        public static RegisterConfigBase BindProperty(this Type type, string propertyName, string configName = null, object handle = null, object defaultValue = null, string description = null)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            var property = Reflection.Property(type, propertyName);
            if (property is null) throw new ArgumentException($"Property {propertyName} does not exist in type {type.FullName}");

            if (string.IsNullOrWhiteSpace(configName)) configName = propertyName;
            if (string.IsNullOrWhiteSpace(description) && property.TryGetAttribute<DescriptionAttribute>(out var descriptionAttribute)) description = descriptionAttribute.Description;

            return new RegisterConfigBase()
            {
                SetValueDelegate = x => SetPropertyBind(property, handle, x),
                GetValueDelegate = () => GetPropertyBind(property, handle),
                DefaultValue = defaultValue,
                Name = configName,
                ValueType = property.PropertyType,
                Description = description
            };
        }

        private static object GetPropertyBind(PropertyInfo property, object handle)
        {
            if (handle is null) return property.Get();
            else return property.Get(handle);
        }

        private static object GetFieldBind(FieldInfo field, object handle)
        {
            if (handle is null) return field.Get();
            else return field.Get(handle);
        }

        private static void SetPropertyBind(PropertyInfo property, object handle, object value)
        {
            if (handle is null) property.Set(value);
            else property.Set(handle, value);
        }

        private static void SetFieldBind(FieldInfo field, object handle, object value)
        {
            if (handle is null) field.Set(value);
            else field.Set(handle, value);
        }
    }
}
