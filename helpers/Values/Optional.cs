namespace helpers.Values
{
    public class Optional<TValue>
    {
        private TValue _value;
        private bool _isNull;

        public bool HasValue { get => !_isNull; }

        public TValue Value
        {
            get
            {
                if (_isNull)
                    return default;

                return _value;
            }
        }

        public void SetValue(TValue value)
        {
            _value = value;
            _isNull = false;
        }

        internal Optional(TValue value) { _value = value; }
        internal Optional() { _isNull = true; }

        public static Optional<TValue> FromValue(TValue value)
            => new Optional<TValue>(value);

        public static Optional<TValue> FromNull()
            => new Optional<TValue>();

        public static void Ensure<T>(ref Optional<T> optional)
        {
            if (optional is null)
                optional = Optional<T>.FromNull();
        }
    }
}