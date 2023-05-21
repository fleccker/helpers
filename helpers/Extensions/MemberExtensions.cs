using System.Reflection;

namespace helpers.Extensions
{
    [LogSource("Member Extensions")]
    public static class MemberExtensions
    {
        public static string GetName(this Assembly assembly) => assembly.GetName().Name;

        public static string ToLogName(this MemberInfo member, bool includeMemberType = true) 
            => includeMemberType ? $"{member.MemberType}: {member.DeclaringType.FullName}::{member.Name}" : $"{member.DeclaringType.FullName}::{member.Name}";
    }
}