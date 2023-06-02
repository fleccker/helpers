using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using helpers.Json;
using helpers.Properties;

namespace helpers
{
    public class Version
    {
        public static Version Current { get; } = FromString(Resources.LibraryVersion);

        [JsonPropertyName("version_minor")]
        public int Major { get; }

        [JsonPropertyName("version_minor")]
        public int Minor { get; }

        [JsonPropertyName("version_build")]
        public int Build { get; }

        [JsonPropertyName("version_patch")]
        public char Patch { get; }

        [JsonPropertyName("version_branch")]
        public string Branch { get; }

        [JsonConstructor]
        public Version(
            int version_major, 
            int version_minor, 
            int version_build, 

            char version_patch, 

            string version_branch)
        {
            Major = version_major;
            Minor = version_minor;
            Build = version_build;

            Patch = version_patch;

            Branch = version_branch;
        }

        public string ToJson(bool indent = true) => JsonHelper.ToJson(this, JsonOptionsBuilder.Default.WithIndented(indent));

        public static Version FromString(string str)
        {
            var parts = str.Split('.');
            var patch = parts.Last().Split('-')[1][0];

            return new Version(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),

                patch,

                "release");
        }

        public static Version FromJson(string json) => JsonHelper.FromJson<Version>(json);
        public static Version FromAssemblyName(AssemblyName assemblyName)
        {
            return new Version(
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build,

                'A',

                "release");
        }

        public static Version FromAssembly(Assembly assembly) => FromAssemblyName(assembly.GetName());
        public static Version FromCurrentAssembly() => FromAssembly(Assembly.GetExecutingAssembly());
        public static Version FromCallingAssembly() => FromAssembly(Assembly.GetCallingAssembly());

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}-{Patch}";
        }
    }
}