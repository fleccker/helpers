using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

using helpers.Json;
using helpers.Properties;

namespace helpers
{
    /// <summary>
    /// Version representation.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Gets the current version.
        /// </summary>
        /// <value>
        /// The current version.
        /// </value>
        public static Version Current { get; } = FromString(Resources.LibraryVersion);

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <value>
        /// The major version number.
        /// </value>
        [JsonPropertyName("version_minor")]
        public int Major { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        [JsonPropertyName("version_minor")]
        public int Minor { get; }

        /// <summary>
        /// Gets the build version number.
        /// </summary>
        [JsonPropertyName("version_build")]
        public int Build { get; }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        [JsonPropertyName("version_patch")]
        public char Patch { get; }

        /// <summary>
        /// Gets the branch.
        /// </summary>
        [JsonPropertyName("version_branch")]
        public string Branch { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="version_major">The version_major.</param>
        /// <param name="version_minor">The version_minor.</param>
        /// <param name="version_build">The version_build.</param>
        /// <param name="version_patch">The version_patch.</param>
        /// <param name="version_branch">The version_branch.</param>
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

        /// <summary>
        /// Serializes this instance to JSON.
        /// </summary>
        /// <param name="indent">If true, indent.</param>
        /// <returns>A JSON string.</returns>
        public string ToJson(bool indent = true) => JsonHelper.ToJson(this, JsonOptionsBuilder.Indented.WithIndented(indent));

        /// <summary>
        /// Gets a new version object from a string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A Version.</returns>
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

        /// <summary>
        /// Creates a new version object from a JSON string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>A Version.</returns>
        public static Version FromJson(string json) => JsonHelper.FromJson<Version>(json);

        /// <summary>
        /// Creates a new version object from an assembly name.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>A Version.</returns>
        public static Version FromAssemblyName(AssemblyName assemblyName)
        {
            return new Version(
                assemblyName.Version.Major,
                assemblyName.Version.Minor,
                assemblyName.Version.Build,

                'A',

                "release");
        }

        /// <summary>
        /// Gets a new version object from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>A Version.</returns>
        public static Version FromAssembly(Assembly assembly) => FromAssemblyName(assembly.GetName());

        /// <summary>
        /// Gets a new version object from the current assembly.
        /// </summary>
        /// <returns>A Version.</returns>
        public static Version FromCurrentAssembly() => FromAssembly(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Gets a new version object from the calling assembly.
        /// </summary>
        /// <returns>A Version.</returns>
        public static Version FromCallingAssembly() => FromAssembly(Assembly.GetCallingAssembly());

        /// <summary>
        /// Converts this version instance to a string.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}-{Patch}";
        }
    }
}