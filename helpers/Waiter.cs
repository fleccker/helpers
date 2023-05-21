using System;
using System.Threading;
using System.Threading.Tasks;

namespace helpers
{
    [LogSource("Waiter")]
    public static class Waiter
    {
        public static void WaitBlock(int time)
        {
            if (time <= 0)
                return;

            Thread.Sleep(time);
        }

        public static void WaitNonBlock(int time, Action callback)
        {
            Task.Run(async () =>
            {
                await Task.Delay(time);

                callback?.Invoke();
            });
        }

        public static void WaitForMethodNonBlock(Action method, Action<bool> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;

                if (timeout != -1)
                {
                    var finished = false;

                    Task.Run(() =>
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

        public static void WaitForMethodNonBlock<T>(Func<T> method, Action<bool, T> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;
                T res = default;

                if (timeout != -1)
                {
                    var finished = false;

                    Task.Run(() =>
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

        public static void WaitForTaskNonBlock(Task task, Action<bool> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;

                if (timeout != -1)
                {
                    var finished = false;

                    Task.Run(async () =>
                    {
                        await task;
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
                    await task;
                }

                callback?.Invoke(true);
            });
        }

        public static void WaitForTaskNonBlock<T>(Task<T> task, Action<bool, T> callback, int timeout = -1)
        {
            Task.Run(async () =>
            {
                var curTime = 0;
                T res = default;

                if (timeout != -1)
                {
                    var finished = false;

                    Task.Run(async () =>
                    {
                        res = await task;
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
                    res = await task;
                }

                callback?.Invoke(true, res);
            });
        }
    }
}
