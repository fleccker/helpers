using System;
using System.Threading;
using System.Threading.Tasks;

namespace helpers
{
    /// <summary>
    ///   <br />
    /// </summary>
    [LogSource("Waiter")]
    public static class Waiter
    {
        /// <summary>
        /// Waits the block.
        /// </summary>
        /// <param name="time">The time.</param>
        public static void WaitBlock(int time)
        {
            if (time <= 0)
                return;

            Thread.Sleep(time);
        }

        /// <summary>
        /// Waits the non block.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="callback">The callback.</param>
        public static void WaitNonBlock(int time, Action callback)
        {
            Task.Run(async () =>
            {
                await Task.Delay(time);

                callback?.Invoke();
            });
        }

        /// <summary>
        /// Waits for method non block.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="timeout">The timeout.</param>
        public static void WaitForMethodNonBlock(Action method, Action<bool> callback, int timeout = -1)
        {
            _ = Task.Run(async () =>
            {
                var curTime = 0;

                if (timeout != -1)
                {
                    var finished = false;

                    _ = Task.Run(() =>
                    {
                        method?.Invoke();
                        finished = true;
                    });

                    while (!finished)
                    {
                        await Task.Delay(10);

                        curTime += 10;

                        if (curTime >= timeout)
                        {
                            callback?.Invoke(false);
                            break;
                        }
                    }
                }
                else
                {
                    method?.Invoke();
                }

                callback?.Invoke(true);
            });
        }

        /// <summary>
        /// Waits the for method non block.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="timeout">The timeout.</param>
        public static void WaitForMethodNonBlock<T>(Func<T> method, Action<bool, T> callback, int timeout = -1)
        {
            _ = Task.Run(async () =>
            {
                var curTime = 0;
                T res = default;

                if (timeout != -1)
                {
                    var finished = false;

                    _ = Task.Run(() =>
                    {
                        res = method.Invoke();
                        finished = true;
                    });

                    while (!finished)
                    {
                        await Task.Delay(10);

                        curTime += 10;

                        if (curTime >= timeout)
                        {
                            callback?.Invoke(false, default);
                            break;
                        }
                    }
                }
                else
                {
                    res = method.Invoke();
                }

                callback?.Invoke(true, res);
            });
        }

        /// <summary>
        /// Waits the for task non block.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="timeout">The timeout.</param>
        public static void WaitForTaskNonBlock(Task task, Action<bool> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;

                if (timeout != -1)
                {
                    var finished = false;

                    _ = Task.Run(async () => { await task; finished = true; });

                    while (!finished)
                    {
                        await Task.Delay(10);

                        curTime += 10;

                        if (curTime >= timeout)
                        {
                            callback?.Invoke(false);
                            break;
                        }
                    }
                }
                else
                {
                    await task;
                }

                callback?.Invoke(true);
            });
        }

        /// <summary>
        /// Waits for task non block.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The task.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="timeout">The timeout.</param>
        public static void WaitForTaskNonBlock<T>(Task<T> task, Action<bool, T> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;
                T res = default;

                if (timeout != -1)
                {
                    var finished = false;

                    _ = Task.Run(async () => { res = await task; finished = true; });

                    while (!finished)
                    {
                        await Task.Delay(10);

                        curTime += 10;
                        if (curTime >= timeout)
                        {
                            callback?.Invoke(false, default);
                            break;
                        }
                    }
                }
                else
                {
                    res = await task;
                }

                callback?.Invoke(true, res);
            });
        }
    }
}
