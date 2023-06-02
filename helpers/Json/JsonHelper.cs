using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace helpers.Json
{
    public static class JsonHelper
    {
        public static string ToJson(this object obj, JsonOptionsBuilder options = null)
        {
            if (options is null) options = JsonOptionsBuilder.Default;
            return JsonSerializer.Serialize(obj, obj.GetType(), options.ToOptions());
        }

        public static async Task<string> ToJsonAsync(this object obj, JsonOptionsBuilder options = null)
        {
            if (options is null) options = JsonOptionsBuilder.Default;
            string serialized = null;

            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                await JsonSerializer.SerializeAsync(memoryStream, obj, options.ToOptions());
                serialized = await reader.ReadToEndAsync();
            }

            return serialized;
        }

        public static string ToJsonNull(this Type type, JsonOptionsBuilder options = null)
        {
            if (options is null) options = JsonOptionsBuilder.Default;
            return JsonSerializer.Serialize((object)null, type, options.ToOptions());
        }

        public static object FromJson(this string json, Type type) => JsonSerializer.Deserialize(json, type);
        public static TType FromJson<TType>(this string json)
        {
            var value = JsonSerializer.Deserialize(json, typeof(TType));
            if (value is null) return default;
            else return (TType)value;
        }
    }
}