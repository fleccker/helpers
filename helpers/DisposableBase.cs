using System;

namespace helpers
{
    public class DisposableBase : IDisposable
    {
        private bool _disposed;

        public bool IsDisposed { get => _disposed; }

        public virtual void Dispose()
        {
            if (_disposed)
                ThrowDisposed();

            _disposed = true;
        }

        public void CheckDisposed()
        {
            if (_disposed)
                ThrowDisposed();
        }

        internal void ThrowDisposed()
            => throw new ObjectDisposedException(GetType().FullName);
    }
}