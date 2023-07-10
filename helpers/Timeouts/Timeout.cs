using System;
using System.Threading.Tasks;

namespace helpers.Timeouts
{
    public struct Timeout
    {
        private DateTime? _endsAt;

        public bool IsTimedOut
        {
            get
            {
                if (!_endsAt.HasValue)
                {
                    Log.Error("You have to set the timeout before accesing it!");
                    return false;
                }

                return DateTime.Now >= _endsAt;
            }
        }

        public void SetEnd(DateTime time)
        {
            _endsAt = time;
        }

        public void Reset()
        {
            _endsAt = null;
        }

        public void AddTime(DateTime time)
        {
            if (_endsAt.HasValue)
                _endsAt = _endsAt.Value.Add(new TimeSpan(time.Ticks));
            else
                _endsAt = time;
        }

        public void AddSeconds(float seconds)
        {
            if (_endsAt.HasValue)
                _endsAt = _endsAt.Value.AddSeconds(seconds);
            else
                _endsAt = DateTime.Now.AddSeconds(seconds);
        }

        public void AddMinutes(float minutes)
        {
            if (_endsAt.HasValue)
                _endsAt = _endsAt.Value.AddMinutes(minutes);
            else
                _endsAt = DateTime.Now.AddMinutes(minutes);
        }

        public void AddMiliseconds(float ms)
        {
            if (_endsAt.HasValue)
                _endsAt = _endsAt.Value.AddMilliseconds(ms);
            else
                _endsAt = DateTime.Now.AddMilliseconds(ms);
        }

        public static void Delay(int delay, Action action)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);

                action?.Invoke();
            });
        }
    }
}