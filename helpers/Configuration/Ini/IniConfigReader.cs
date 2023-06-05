using System.Text;

namespace helpers.Configuration.Ini
{
    [LogSource("Ini Reader")]
    public class IniConfigReader
    {
        private string[] _buffer;
        private string _curKeyLine;
        private int _curPos;

        public string CurrentKey { get; private set; }
        public string CurrentValue { get; private set; }

        public bool IsLineValid { get; private set; }

        public IniConfigReader(string[] buffer, int startPos = 0)
        {
            _buffer = buffer;
            _curPos = startPos;
        }

        public bool TryMove()
        {
            if (_curPos >= _buffer.Length)
            {
                return false;
            }

            _curKeyLine = _buffer[_curPos].Trim();
            _curPos++;

            CurrentKey = null;
            CurrentValue = null;
            IsLineValid = false;

            if (string.IsNullOrWhiteSpace(_curKeyLine) || _curKeyLine.StartsWith("#"))
            {
                return true;
            }

            if (_curKeyLine != "[]" 
                && _curKeyLine.StartsWith("[") 
                && _curKeyLine.EndsWith("]"))
            {
                LoadKey();
                LoadValue();

                IsLineValid = true;
            }

            return true;
        }

        private void LoadKey()
        {
            CurrentKey = _curKeyLine
                .Replace("[", "")
                .Replace("]", "")
                .Trim();
        }

        private void LoadValue()
        {
            var valueIndex = _curPos;
            var valueBuilder = new StringBuilder();

            while (!(valueIndex >= _buffer.Length))
            {
                if (string.IsNullOrWhiteSpace(_buffer[valueIndex]) ||
                    _buffer[valueIndex].StartsWith("#") ||
                    (_buffer[valueIndex].StartsWith("[") && _buffer[valueIndex].EndsWith("]") && _buffer[valueIndex] != "[]"))
                {
                    break;
                }

                valueBuilder.AppendLine(_buffer[valueIndex]);
                valueIndex++;
            }

            CurrentValue = valueBuilder.ToString();
        }
    }
}