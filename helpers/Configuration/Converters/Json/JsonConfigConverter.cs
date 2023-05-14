using Newtonsoft.Json;
using System;

namespace helpers.Configuration.Converters.Json
{
    public class JsonConfigConverter : IConfigConverter
    {
        public bool Indent { get; set; }

        public string TypeName { get; } = "JSON";

        public JsonConfigConverter() 
        {
            Indent = true;
        }

        public JsonConfigConverter(bool indent)
        {
            Indent = indent;
        }

        public bool TryConvert(string value, Type type, out object result)
        {
            if (value == "null")
                result = null;
            else
                result = JsonConvert.DeserializeObject(value, type);

            return true;
        }

        public bool TryConvert(object value, out string result)
        {
            if (value is null)
                result = "null";
            else
                result = JsonConvert.SerializeObject(value, Indent ? Formatting.Indented : Formatting.None);

            return true;
        }
    }
}
