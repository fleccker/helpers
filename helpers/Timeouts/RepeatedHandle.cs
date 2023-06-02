using System;
using System.Threading;

namespace helpers.Timeouts
{
    public struct RepeatedHandle
    {
        private Action _delegate;
        private Timeout _timeout;
        private Thread _thread;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;

        public float Delay
        {
            get => _timeout.Value;
            set
            {
                var timeout = _timeout;
                timeout.Value = value;
                _timeout = timeout;
            }
        }

        public bool IsRunning { get; private set; }

        public RepeatedHandle(float value, Action handle)
        {
            _delegate = handle;
            _timeout = new Timeout(value);
            _thread = new Thread(Update);
            _thread.Start();
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;
        public void Stop()
        {
            Pause();

            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
            _thread = null;
            _delegate = null;
        }

        public void Execute()
        {
            _delegate?.Invoke();
            _timeout.Execute();
        }

        private void Update()
        {
            while (true)
            {
                _token.ThrowIfCancellationRequested();

                if (!IsRunning) continue;
                if (!_timeout.CanExecute) continue;

                _delegate?.Invoke();
                _timeout.Execute();
            }
        }
    }
}