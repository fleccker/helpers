using Newtonsoft.Json;

using System.Linq;
using System.Reflection;

using helpers.Properties;

namespace helpers
{
    public class Version
    {
        public static Version Current { get; } = FromString(Resources.LibraryVersion);

        [JsonProperty("version_minor")]
        public int Major { get; }

        [JsonProperty("version_minor")]
        public int Minor { get; }

        [JsonProperty("version_build")]
        public int Build { get; }

        [JsonProperty("version_patch")]
        public char Patch { get; }

        [JsonProperty("version_branch")]
        public string Branch { get; }

        [JsonConstructor]
        public Version(
            [JsonProperty("version_minor")] int major, 
            [JsonProperty("version_minor")] int minor, 
            [JsonProperty("version_build")] int build, 

            [JsonProperty("version_patch")] char patch, 

            [JsonProperty("version_brach")] string branch)
        {
            Major = major;
            Minor = minor;
            Build = build;

            Patch = patch;

            Branch = branch;
        }

        public string ToJson(bool indent = true)
            => JsonConvert.SerializeObject(this, indent ? Formatting.Indented : Formatting.None);

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

        public static Version FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Version>(json);
        }

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