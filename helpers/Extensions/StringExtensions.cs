using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnitsNet.Units;
using UnitsNet;

using helpers.IO;

namespace helpers.Extensions
{
    [LogSource("String Extensions")]
    public static class StringExtensions
    {
        public const string NewLinesRegex = "r\n|\r|\n";
        public const string PascalCaseRegex = "([a-z,0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        public static string SpaceByPascalCase(this string str) => Regex.Replace(str, PascalCaseRegex, "$1 ", RegexOptions.Compiled);

        public static string RemovePathUnsafe(this string value) => value.Remove(FileManager.PathIllegalCharacters);

        public static string Remove(this string value, IEnumerable<char> toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace($"{c}", "");

            return value;
        }

        public static string Remove(this string value, IEnumerable<string> toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace(c, "");

            return value;
        }

        public static string Remove(this string value, params char[] toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace($"{c}", "");

            return value;
        }

        public static string Remove(this string value, params string[] toRemove)
        {
            foreach (var str in toRemove)
                value = value.Replace(str, "");

            return value;
        }

        public static string ReplaceWithMap(this string value, params KeyValuePair<string, string>[] stringMap) => value.ReplaceWithMap(stringMap.ToDictionary());
        public static string ReplaceWithMap(this string value, params KeyValuePair<char, string>[] charMap) => value.ReplaceWithMap(charMap.ToDictionary());
        public static string ReplaceWithMap(this string value, params KeyValuePair<char, char>[] charMap) => value.ReplaceWithMap(charMap.ToDictionary());

        public static string ReplaceWithMap(this string value, IDictionary<char, string> charMap)
        {
            foreach (var pair in charMap)
            {
                value = value.Replace(pair.Key.ToString(), pair.Value);
            }

            return value;
        }

        public static string ReplaceWithMap(this string value, IDictionary<char, char> charMap)
        {
            foreach (var pair in charMap)
            {
                value = value.Replace(pair.Key, pair.Value);
            }

            return value;
        }

        public static string ReplaceWithMap(this string value, IDictionary<string, string> stringMap)
        {
            foreach (var pair in stringMap)
            {
                value = value.Replace(pair.Key, pair.Value);
            }

            return value;
        }

        public static string[] SplitLines(this string line)
        {
            return Regex.Split(line, NewLinesRegex);
        }

        public static string GetAfterIndex(this string str, int index)
        {
            string s = "";

            for (int i = index; i < str.Length; i++)
            {
                s += str[i];
            }

            return s;
        }

        public static string GetBeforeIndex(this string str, int index)
        {
            string s = "";

            for (int i = 0; i < index; i++)
            {
                s += str[i];
            }

            return s;
        }

        public static string CutByChar(this string str, char c, int lastIndex, out (int startIndex, int endIndex) index)
        {
            index = 0;
            
            int startIndex = 0;
            int endIndex = 0;

            for (int i = lastIndex; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    if (startIndex == 0)
                        startIndex = i;
                    else
                        endIndex = i;
                }

                if (startIndex != 0 && endIndex != 0)
                    break;
            }

            index.startIndex = startIndex;
            index.endIndex = endIndex;

            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static string[] GetByChar(this string str, char c)
        {
            int lastIndex = 0;
            int strLastIndex = str.LastIndexOf(c);

            List<string> strs = new List<string>();

            while (strLastIndex != lastIndex)
            {
                strs.Add(CutByChar(str, c, lastIndex, out var indexes));

                lastIndex = indexes.endIndex;
            }

            return strs.ToArray();
        }

        public static string FloorFrequencyToString(this double cpuFrequencyMHz)
        {
            Frequency freq = new Frequency(cpuFrequencyMHz, FrequencyUnit.Megahertz);

            if (freq.Gigahertz > 0)
                return $"{Math.Floor(freq.Gigahertz)} GHz";
            else
                return $"{Math.Floor(freq.Megahertz)} MHz";
        }
    }
}