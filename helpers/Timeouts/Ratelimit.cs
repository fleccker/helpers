using System;

namespace helpers.Timeouts
{
    public struct Ratelimit
    {
        private DateTime _lastCheckTime;
        private DateTime _lastExecTime;

        public float Value { get; set; }

        public bool CanExecute => (DateTime.Now - _lastExecTime).TotalMilliseconds >= Value;

        public Ratelimit(float value) => Value = value;

        public void Execute()
        {
            _lastCheckTime = DateTime.Now;
            _lastExecTime = _lastCheckTime;
        }
    }
}