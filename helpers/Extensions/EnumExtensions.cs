using EnumsNET;

using System;
using System.Linq;

namespace helpers.Extensions
{
    [LogSource("Enum Extensions")]
    public static class EnumExtensions
    {
        public static T AddFlags<T>(this T currentFlags, params T[] flags) where T : struct, Enum
        {
            return FlagEnums.CombineFlags(currentFlags, FlagEnums.CombineFlags(flags));
        }

        public static T AddFlag<T>(this T currentFlags, T flag) where T : struct, Enum
        {
            return FlagEnums.CombineFlags(currentFlags, flag);
        }

        public static T RemoveFlags<T>(T currentFlags, params T[] flags) where T : struct, Enum
        {
            return FlagEnums.RemoveFlags(currentFlags, FlagEnums.CombineFlags(flags));
        }

        public static T RemoveFlag<T>(this T currentFlags, T flag) where T : struct, Enum
        {
            return FlagEnums.RemoveFlags(currentFlags, flag);
        }

        public static T GetAllFlags<T>() where T : struct, Enum
        {
            return FlagEnums.GetAllFlags<T>();
        }

        public static T[] GetAllFlagsArray<T>() where T : struct, Enum
        {
            return FlagEnums.GetFlagMembers(GetAllFlags<T>()).Select(x => x.Value).ToArray();
        }

        public static T FromString<T>(this string str) where T : struct, Enum
        {
            return FlagEnums.ParseFlags<T>(str);
        }

        public static short ToInt16<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (short)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt16();

            return totalValue;
        }

        public static ushort ToUInt16<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (ushort)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt16();

            return totalValue;
        }

        public static int ToInt32<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (int)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt32();

            return totalValue;
        }

        public static long ToInt64<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (long)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt64();

            return totalValue;
        }

        public static uint ToUInt32<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (uint)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt32();

            return totalValue;
        }

        public static ulong ToUInt64<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (ulong)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt64();

            return totalValue;
        }

        public static byte ToByte<T>(this T flags) where T : struct, Enum
        {
            var totalValue = byte.MinValue;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToByte();

            return totalValue;
        }

        public static sbyte ToSByte<T>(this T flags) where T : struct, Enum
        {
            var totalValue = (sbyte)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToSByte();

            return totalValue;
        }

        public static string ToString<T>(this T flags) where T : struct, Enum
        {
            return FlagEnums.FormatFlags(flags);
        }

        public static bool IsValidFlags<T>(this T flags) where T : struct, Enum
        {
            return FlagEnums.IsValidFlagCombination(flags);
        }

        public static bool IsFlagEnum<T>() where T : struct, Enum
        {
            return FlagEnums.IsFlagEnum<T>();
        }
    }
}
