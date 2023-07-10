using helpers.Extensions;
using helpers.Random;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace helpers.Events
{
    public class EventProvider
    {
        private readonly List<Delegate> m_Handlers = new List<Delegate>();
        private readonly string m_Id;

        private int m_Times = 0;
        private bool m_Allowed;

        public IReadOnlyList<Delegate> Handlers => m_Handlers;

        public int TimesExecuted => m_Times;

        public string Id => m_Id;

        public bool IsAllowed { get => m_Allowed; set => m_Allowed = value; }

        public EventProvider(string id = null)
        {
            m_Allowed = true;

            if (id is null)
            {
                m_Id = RandomGeneration.Default.GetReadableString(20);
            }
            else
            {
                m_Id = id;
            }
        }

        public void Invoke(params object[] args)
        {
            if (!m_Allowed)
                return;

            m_Times++;
            m_Handlers.ForEach(target =>
            {
                try
                {
                    if (!ValidateArgs(args, target.Method.GetParameters(), out var newArgs))
                        return;

                    target.Method.Invoke(target.Target, newArgs);
                }
                catch (Exception ex)
                {
                    Log.Error($"Event Handler ({Id})", $"Failed to execute target: {target.Method.Name} ({target.Method.DeclaringType.FullName})\n{ex}");
                }
            });
        }

        public EventResult<TResult> InvokeResult<TResult>(TResult defResult, params object[] args)
        {
            if (!m_Allowed)
                return null;

            m_Times++;

            var result = new EventResult<TResult>() { Result = defResult };

            m_Handlers.ForEach(target =>
            {
                try
                {
                    if (!ValidateArgs(args, target.Method.GetParameters(), result, out var newArgs))
                        return;

                    target.Method.Invoke(target.Target, newArgs);
                }
                catch (Exception ex)
                {
                    Log.Error($"Event Handler ({Id})", $"Failed to execute target: {target.Method.Name} ({target.Method.DeclaringType.FullName})\n{ex}");
                }
            });

            return result;
        }

        public void Register(Delegate target)
        {
            m_Handlers.Add(target);
        }

        public void Unregister(Delegate target)
        {
            m_Handlers.Remove(target);
        }

        public void UnregisterAll()
        {
            m_Handlers.Clear();
        }

        private bool ValidateArgs(object[] args, ParameterInfo[] parameters, out object[] newArgs)
        {
            if (parameters is null || !parameters.Any())
            {
                newArgs = null;
                return true;
            }

            Log.Debug($"Validating arguments: {args.Length} / {parameters.Length}");

            newArgs = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Log.Debug($"Index {i}: {args[i]} / {parameters[i].ParameterType.FullName}");

                if (!(i >= args.Length))
                {
                    newArgs[i] = args[i];
                    continue;
                }
                else
                {
                    newArgs[i] = null;
                    continue;
                }
            }

            return true;
        }

        private bool ValidateArgs<TResult>(object[] args, ParameterInfo[] parameters, EventResult<TResult> result, out object[] newArgs)
        {
            if (parameters is null || !parameters.Any())
            {
                newArgs = null;
                return true;
            }    

            if (parameters.Length is 1 && parameters[0].ParameterType == typeof(EventResult<TResult>))
            {
                newArgs = new object[] { result };
                return true;
            }

            newArgs = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(EventResult<TResult>))
                {
                    newArgs[i] = result;
                    continue;
                }

                if (!(i >= args.Length))
                {
                    newArgs[i] = args[i];
                    continue;
                }
                else
                {
                    newArgs[i] = null;
                    continue;
                }
            }

            return true;
        }
    }
}