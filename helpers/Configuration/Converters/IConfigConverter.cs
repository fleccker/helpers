using System;

namespace helpers.Configuration.Converters
{
    public interface IConfigConverter
    {
        string TypeName { get; }

        bool TryConvert(string value, Type type, out object result);
        bool TryConvert(object value, out string result);
    }
}