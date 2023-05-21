using System;

namespace helpers.Callbacks
{
    public class Callback
    {
        private Action<CallbackCollection> _callback;

        public Callback(Action<CallbackCollection> callback)
        {
            _callback = callback;
        }

        public void Execute(CallbackCollection callback)
        {
            _callback?.Invoke(callback);
        }
    }
}