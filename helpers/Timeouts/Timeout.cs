using System;

namespace helpers.Timeouts
{
    public struct Timeout
    {
        private DateTime _lastCheckTime;
        private DateTime _lastExecTime;

        public float Value { get; set; }
        public bool CanExecute => (DateTime.Now - _lastExecTime).TotalMilliseconds >= Value;

        public Timeout(float value) => Value = value;

        public void Execute()
        {
            _lastCheckTime = DateTime.Now;
            _lastExecTime = _lastCheckTime;
        }
    }
}