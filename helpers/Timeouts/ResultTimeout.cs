using System;
using System.Threading.Tasks;

namespace helpers.Timeouts
{
    public struct ResultTimeout<TResult>
    {
        private readonly Func<TResult> m_ResultGetter;
        private readonly Func<TResult, bool> m_ResultValidator;
        private readonly Action m_Callback;

        private TResult m_ResultValue;
        private ResultTimeoutStatus m_Status;

        private readonly Timeout m_Timeout;

        public Timeout Timeout => m_Timeout;

        public ResultTimeoutStatus Status => m_Status;

        public TResult Result => m_ResultValue;

        public ResultTimeout(Func<TResult> getter, Func<TResult, bool> validator, Timeout timeout, float pullRate = 100f, Action callback = null)
        {
            m_ResultGetter = getter;
            m_ResultValidator = validator;
            m_Timeout = timeout;
            m_Callback = callback;

            Runner(pullRate);
        }

        private void Runner(float pullRate)
        {
            var timeoutRes = this;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (timeoutRes.Check())
                        break;

                    await Task.Delay((int)pullRate);
                }

                if (timeoutRes.m_Callback != null)
                {
                    timeoutRes.m_Callback();
                }
            });
        }

        private bool Check()
        {
            m_ResultValue = m_ResultGetter();

            if (m_ResultValidator(m_ResultValue))
            {
                m_Status = ResultTimeoutStatus.ValidResult;
                return true;
            }

            if (m_Timeout.IsTimedOut)
            {
                m_Status = ResultTimeoutStatus.TimedOut;
                return true;
            }

            m_Status = ResultTimeoutStatus.Running;
            return false;
        }
    }
}