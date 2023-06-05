using helpers.Configuration.Converters;
using helpers.Extensions;

using System.Collections.Generic;
using System.Text;

namespace helpers.Configuration.Register
{
    public class RegisterConfigReader
    {
        private readonly IConfigConverter _converter;
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public RegisterConfigReader(IConfigConverter converter) => _converter = converter;

        public void ReadAll(IEnumerable<RegisterConfigBase> registers)
        {
            foreach (var value in _values)
            {
                if (!registers.TryGetFirst(x => x.Name == value.Key, out var register))
                {
                    Log.Warn("Config Reader", $"Missing config register for key: {value.Key}");
                    continue;
                }

                if (!_converter.TryConvert(value.Value, register.ValueType, out var result))
                {
                    Log.Warn($"Config Reader", $"Failed to convert {value.Key} to {register.ValueType.FullName}!");
                    continue;
                }

                register.SetValue(result);

                Log.Debug($"Config Reader", $"Assigned value to {register.Name}");
            }
        }

        public void ReadConfig(string[] content)
        {
            _values.Clear();

            var isBuildingValue = false;

            var currentKey = "";
            var currentValue = new StringBuilder();

            for (int i = 0; i < content.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(content[i])) continue;
                if (content[i].StartsWith("#")) continue;
                if (isBuildingValue)
                {
                    currentValue.AppendLine(content[i]);

                    if (content.TryPeekIndex(i + 1, out var nextValue) && nextValue.StartsWith("[") && nextValue.EndsWith("]") && nextValue != "[]")
                    {
                        isBuildingValue = false;
                        _values[currentKey] = currentValue.ToString();
                        currentValue.Clear();
                        continue;
                    }
                }
                else
                {
                    if (content[i].StartsWith("[") && content[i].EndsWith("]") && content[i] != "[]")
                    {
                        currentKey = content[i].TrimStart('[').TrimEnd(']').Trim();
                        isBuildingValue = true;
                        continue;
                    }
                }
            }
        }
    }
}