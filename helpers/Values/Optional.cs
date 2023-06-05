namespace helpers.Values
{
    [LogSource("Optional Value")]
    public struct Optional<TValue>
    {
        private TValue _value;
        private bool _isNull = true;

        public bool HasValue => !_isNull;
        public TValue Value
        {
            get
            {
                if (_isNull) return default;
                else return _value;
            }
        }

        public void SetValue(TValue value)
        {
            _value = value;
            _isNull = value is null;
        }

        internal Optional(TValue value) => _value = value;

        public static Optional<TValue> FromValue(TValue value) => new Optional<TValue>(value);
        public static Optional<TValue> Null => new Optional<TValue>();
    }
}