using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace helpers.Json
{
    public class JsonOptionsBuilder
    {
        public static JsonOptionsBuilder Default => new JsonOptionsBuilder();

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
            IncludeFields = true,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace,
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            ReferenceHandler = ReferenceHandler.Preserve,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            WriteIndented = true
        };

        public JsonOptionsBuilder WithIndented(bool indent = true)
        {
            _options.WriteIndented = indent;
            return this;
        }

        public JsonOptionsBuilder Modify(Action<JsonSerializerOptions> modify)
        {
            modify?.Invoke(_options);
            return this;
        }

        public JsonSerializerOptions ToOptions() => _options;
    }
}