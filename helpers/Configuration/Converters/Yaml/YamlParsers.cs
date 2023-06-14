using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace helpers.Configuration.Converters.Yaml
{
    public static class YamlParsers
    {
        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .DisableAliases()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitDefaults)
            .IncludeNonPublicProperties()
            .IgnoreFields()
            .JsonCompatible()
            .Build();

        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .IgnoreFields()
            .IncludeNonPublicProperties()
            .Build();
    }
}