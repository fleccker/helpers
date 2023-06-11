using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnitsNet.Units;
using UnitsNet;

using helpers.IO;
using helpers.Parsers.String;

namespace helpers.Extensions
{
    [LogSource("String Extensions")]
    public static class StringExtensions
    {
        public const string NewLinesRegex = "r\n|\r|\n";
        public const string PascalCaseRegex = "([a-z,0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        public static string SpaceByPascalCase(this string str) => Regex.Replace(str, PascalCaseRegex, "$1 ", RegexOptions.Compiled);

        public static bool HasHtmlTags(this string text, out IList<int> openIndexes, out IList<int> closeIndexes)
        {
            openIndexes = Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
            closeIndexes = Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();

            return openIndexes.Any() || closeIndexes.Any();
        }

        public static string RemoveHtmlTags(this string text, IList<int> openTagIndexes = null, IList<int> closeTagIndexes = null)
        {
            openTagIndexes ??= Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
            closeTagIndexes ??= Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();

            if (closeTagIndexes.Count > 0)
            {
                var sb = new StringBuilder();
                var previousIndex = 0;

                foreach (int closeTagIndex in closeTagIndexes)
                {
                    var openTagsSubset = openTagIndexes.Where(x => x >= previousIndex && x < closeTagIndex);

                    if (openTagsSubset.Count() > 0 && closeTagIndex - openTagsSubset.Max() > 1)
                    {
                        sb.Append(text.Substring(previousIndex, openTagsSubset.Max() - previousIndex));
                    }
                    else
                    {
                        sb.Append(text.Substring(previousIndex, closeTagIndex - previousIndex + 1));
                    }

                    previousIndex = closeTagIndex + 1;
                }

                if (closeTagIndexes.Max() < text.Length)
                {
                    sb.Append(text.Substring(closeTagIndexes.Max() + 1));
                }

                return sb.ToString();
            }
            else
            {
                return text;
            }
        }

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

        public static double GetSimilarity(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            var stepsToSame = GetLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public static int GetLevenshteinDistance(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            var sourceWordCount = source.Length;
            var targetWordCount = target.Length;

            if (sourceWordCount == 0) return targetWordCount;
            if (targetWordCount == 0) return sourceWordCount;

            var distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;
            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        public static bool TrySplit(this string line, char splitChar, bool removeEmptyOrWhitespace, int? length, out string[] splits)
        {
            splits = line.Split(splitChar).Select(str => str.Trim()).ToArray();

            if (removeEmptyOrWhitespace)
                splits = splits.Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

            if (length.HasValue && splits.Length != length)
                return false;

            return splits.Any();
        }

        public static bool TrySplit(this string line, char[] splitChars, bool removeEmptyOrWhitespace, int? length, out string[] splits)
        {
            splits = line.Split(splitChars).Select(str => str.Trim()).ToArray(); 

            if (removeEmptyOrWhitespace)
                splits = splits.Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

            if (length.HasValue && splits.Length != length)
                return false;

            return splits.Any();
        }

        public static bool TryParse(this string input, out string[] parts) => StringParser.TryParse(input, out parts);

        public static string[] SplitLines(this string line)
        {
            return Regex.Split(line, NewLinesRegex);
        }

        public static string Between(this string str, int start, int end)
        {
            var res = "";

            start++;
            end--;

            if (end - start <= 0)
                return null;

            for (int i = start + 1; i < end - 1; i++)
            {
                res += str[i];
            }

            return res;
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