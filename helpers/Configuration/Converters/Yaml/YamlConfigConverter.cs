using System;

namespace helpers.Configuration.Converters.Yaml
{
    public class YamlConfigConverter : IConfigConverter
    {
        public string TypeName => "YAML";

        public bool TryConvert(string value, Type type, out object result)
        {
            if (value == "null") result = null;
            else result = YamlParsers.Deserializer.Deserialize(value, type);
            return true;
        }

        public bool TryConvert(object value, out string result)
        {
            if (value is null) result = "null";
            else result = YamlParsers.Serializer.Serialize(value);
            return !string.IsNullOrWhiteSpace(result);
        }
    }
}