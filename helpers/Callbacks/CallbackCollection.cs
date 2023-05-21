using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace helpers.Callbacks
{
    public class CallbackCollection
    {
        private Dictionary<Type, List<object>> _callbackArguments = new Dictionary<Type, List<object>>();
        private MethodBase _lockedMethod;

        public CallbackCollection Add(object arg)
        {
            _callbackArguments[arg.GetType()].Add(arg);
            return this;
        }

        public CallbackCollection MethodLock()
        {
            var callingMethod = new StackTrace().GetFrame(1).GetMethod();

            if (_lockedMethod != null)
            {
                if (_lockedMethod != callingMethod)
                {
                    return this;
                }
            }

            _lockedMethod = callingMethod;
            return this;
        }
    }
}