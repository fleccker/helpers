using EnumsNET;

using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Values
{
    public class FlagEnumValue<T> where T : struct, Enum
    {
        private T _flagValue;
        private T _defaultFlags;

        public IReadOnlyList<T> AllFlagsList { get; }
        public T AllFlags { get; }

        public int CurrentFlagsCount { get => FlagEnums.GetFlagCount(_flagValue); }
        public int AllFlagsCount { get => FlagEnums.GetFlagCount(AllFlags); }

        public bool HasAllFlags
        {
            get
            {
                EnsureValid();
                return FlagEnums.HasAllFlags(_flagValue);
            }
        }

        public bool HasAnyFlags
        {
            get
            {
                EnsureValid();
                return FlagEnums.HasAnyFlags(_flagValue);
            }
        }

        public bool IsValid
        {
            get
            {
                return FlagEnums.IsValidFlagCombination(_flagValue);
            }
        }

        public FlagEnumValue()
        {
            if (!FlagEnums.IsFlagEnum<T>())
            {
                throw new InvalidOperationException($"{typeof(T).FullName} is not a Flag!");
            }

            AllFlags = FlagEnums.GetAllFlags<T>();
            AllFlagsList = FlagEnums.GetFlagMembers(AllFlags).Select(x => x.Value).ToList();

            _defaultFlags = default;
        }

        public FlagEnumValue(T defaultFlags)
        {
            if (!FlagEnums.IsFlagEnum<T>())
            {
                throw new InvalidOperationException($"{typeof(T).FullName} is not a Flag!");
            }

            AllFlags = FlagEnums.GetAllFlags<T>();
            AllFlagsList = FlagEnums.GetFlagMembers(AllFlags).Select(x => x.Value).ToList();

            _defaultFlags = defaultFlags;
        }

        public T Flags
        {
            get
            {
                EnsureValid();
                return _flagValue;
            }
            set
            {
                _flagValue = value;
                EnsureValid();
            }
        }

        public T DefaultFlags
        {
            get
            {
                return _defaultFlags;
            }
            set
            {
                _defaultFlags = value;
            }
        }

        public FlagEnumValue<T> WithDefaultFlags(T flags)
        {
            _defaultFlags = flags;
            return this;
        }

        public FlagEnumValue<T> SetDefaultFlags()
        {
            _flagValue = _defaultFlags;
            return this;
        }

        public FlagEnumValue<T> WithParsedFlag(string flagString)
        {
            _flagValue = FlagEnums.ParseFlags<T>(flagString);

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithParsedFlags(params string[] flagStrings)
        {
            _flagValue = FlagEnums.CombineFlags(_flagValue, FlagEnums.CombineFlags(flagStrings.Select(x => FlagEnums.ParseFlags<T>(x))));

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithoutParsedFlag(string flagString)
        {
            _flagValue = FlagEnums.RemoveFlags(_flagValue, FlagEnums.ParseFlags<T>(flagString));

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithoutParsedFlags(params string[] parsedFlags)
        {
            _flagValue = FlagEnums.RemoveFlags(_flagValue, FlagEnums.CombineFlags(parsedFlags.Select(x => FlagEnums.ParseFlags<T>(x))));

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithFlag(T flag)
        {
            _flagValue = FlagEnums.CombineFlags(_flagValue, flag);

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithFlags(params T[] flags)
        {
            _flagValue = FlagEnums.CombineFlags(_flagValue, FlagEnums.CombineFlags(flags));

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithoutFlag(T flag)
        {
            _flagValue = FlagEnums.RemoveFlags(_flagValue, flag);

            EnsureValid();
            return this;
        }

        public FlagEnumValue<T> WithoutFlags(params T[] flags)
        {
            _flagValue = FlagEnums.RemoveFlags(_flagValue, FlagEnums.CombineFlags(flags));

            EnsureValid();
            return this;
        }

        public short ToInt16()
        {
            EnsureValid();

            var totalValue = (short)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToInt16();

            return totalValue;
        }

        public ushort ToUInt16()
        {
            EnsureValid();

            var totalValue = (ushort)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToUInt16();

            return totalValue;
        }

        public int ToInt32()
        {
            EnsureValid();

            var totalValue = (int)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToInt32();

            return totalValue;
        }

        public long ToInt64()
        {
            EnsureValid();

            var totalValue = (long)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToInt64();

            return totalValue;
        }

        public uint ToUInt32()
        {
            EnsureValid();

            var totalValue = (uint)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToUInt32();

            return totalValue;
        }

        public ulong ToUInt64()
        {
            EnsureValid();

            var totalValue = (ulong)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToUInt64();

            return totalValue;
        }

        public byte ToByte()
        {
            EnsureValid();

            var totalValue = byte.MinValue;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToByte();

            return totalValue;
        }

        public sbyte ToSByte()
        {
            EnsureValid();

            var totalValue = (sbyte)0;
            var members = FlagEnums.GetFlagMembers(_flagValue);

            foreach (var member in members)
                totalValue &= member.ToSByte();

            return totalValue;
        }

        public override string ToString()
        {
            EnsureValid();
            return FlagEnums.FormatFlags(_flagValue) ?? "";
        }

        private void EnsureValid()
        {
            if (!FlagEnums.IsValidFlagCombination(_flagValue))
                _flagValue = _defaultFlags;
        }

        public static T AddFlags(T currentFlags, params T[] flags)
        {
            return FlagEnums.CombineFlags(currentFlags, FlagEnums.CombineFlags(flags));
        }

        public static T AddFlag(T currentFlags, T flag) 
        {
            return FlagEnums.CombineFlags(currentFlags, flag);
        }

        public static T RemoveFlags(T currentFlags, params T[] flags) 
        {
            return FlagEnums.RemoveFlags(currentFlags, FlagEnums.CombineFlags(flags));
        }

        public static T RemoveFlag(T currentFlags, T flag)
        {
            return FlagEnums.RemoveFlags(currentFlags, flag);
        }

        public static T GetAllFlags()
        {
            return FlagEnums.GetAllFlags<T>();
        }

        public static T[] GetAllFlagsArray()
        {
            return FlagEnums.GetFlagMembers(GetAllFlags()).Select(x => x.Value).ToArray();
        }

        public static T FromString(string str)
        {
            return FlagEnums.ParseFlags<T>(str);
        }

        public static short ToInt16(T flags)
        {
            var totalValue = (short)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt16();

            return totalValue;
        }

        public static ushort ToUInt16(T flags)
        {
            var totalValue = (ushort)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt16();

            return totalValue;
        }

        public static int ToInt32(T flags)
        {
            var totalValue = (int)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt32();

            return totalValue;
        }

        public static long ToInt64(T flags)
        {
            var totalValue = (long)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToInt64();

            return totalValue;
        }

        public static uint ToUInt32(T flags)
        {
            var totalValue = (uint)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt32();

            return totalValue;
        }

        public static ulong ToUInt64(T flags)
        {
            var totalValue = (ulong)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToUInt64();

            return totalValue;
        }

        public static byte ToByte(T flags)
        {
            var totalValue = byte.MinValue;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToByte();

            return totalValue;
        }

        public static sbyte ToSByte(T flags)
        {
            var totalValue = (sbyte)0;
            var members = FlagEnums.GetFlagMembers(flags);

            foreach (var member in members)
                totalValue &= member.ToSByte();

            return totalValue;
        }

        public static string ToString(T flags)
        {
            return FlagEnums.FormatFlags(flags);
        }

        public static bool IsValidFlags(T flags)
        {
            return FlagEnums.IsValidFlagCombination(flags);
        }

        public static bool IsFlagEnum()
        {
            return FlagEnums.IsFlagEnum<T>();
        }
    }
}
