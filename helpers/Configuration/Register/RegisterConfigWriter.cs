using helpers.Configuration.Converters;
using helpers.Extensions;

using System.Text;
using System;
using System.Collections.Generic;

namespace helpers.Configuration.Register
{
    public class RegisterConfigWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly IConfigConverter _converter;

        public RegisterConfigWriter(IConfigConverter converter) => _converter = converter;

        public void WriteAll(IEnumerable<RegisterConfigBase> registers) => registers.ForEach(Write);
        public void Write(RegisterConfigBase registerConfigBase)
        {
            _builder.AppendLine();
            if (!string.IsNullOrWhiteSpace(registerConfigBase.Description)) _builder.AppendLine($"# {registerConfigBase.Description}");
            var value = registerConfigBase.GetValue();
            _builder.AppendLine($"[{registerConfigBase.Name}]");
            if (value is null) _builder.AppendLine("null");
            if (!_converter.TryConvert(value, out var result)) throw new Exception($"Failed to convert {value} to a string.");
            _builder.AppendLine(result);
        }

        public override string ToString()
        {
            var str = _builder.ToString();
            _builder.Clear();
            return str;
        }
    }
}
