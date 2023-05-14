using helpers.Configuration.Converters;

using System;
using System.Text;

namespace helpers.Configuration
{
    public class IniConfigWriter
    {
        private IConfigConverter _converter;
        private StringBuilder _builder;
        private bool _insertedEnding;

        public IniConfigWriter(IConfigConverter converter)
        {
            if (converter is null)
                throw new ArgumentNullException(nameof(converter));

            _converter = converter;
            _builder = new StringBuilder();
        }

        public IniConfigWriter WriteObject(string objectKey, string[] keyDescription, object obj)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
                throw new ArgumentNullException(nameof(objectKey));

            if (_converter.TryConvert(obj, out var converted))
            {
                if (keyDescription != null && keyDescription.Length > 0)
                {
                    foreach (var description in keyDescription)
                    {
                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            _builder.AppendLine($"# {description}");
                        }
                    }
                }

                _builder.AppendLine($"[{objectKey}]");

                if (!string.IsNullOrWhiteSpace(converted))
                {
                    _builder.AppendLine(converted);
                }
            }

            _builder.AppendLine();

            return this;
        }

        public override string ToString()
        {
            if (!_insertedEnding)
            {
                _builder.AppendLine($"# Generated at {DateTime.Now.ToString("G")} using the {_converter.TypeName} converter.");
                _insertedEnding = true;
            }

            return _builder.ToString();
        }
    }
}