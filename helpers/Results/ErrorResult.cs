using System;

namespace helpers.Results
{
    public struct ErrorResult : IResult<object>
    {
        public bool IsSuccess { get; }
        public string Reason { get; }
        public object Result { get; }

        public Exception Exception { get; }

        public ErrorResult(string reason, Exception exception = null)
        {
            Reason = reason;
            Exception = exception;
            Result = null;
            IsSuccess = false;
        }
    }

    public struct ErrorResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess { get; }
        public string Reason { get; }
        public TResult Result { get; }

        public Exception Exception { get; }

        public ErrorResult(string reason, Exception exception = null)
        {
            Reason = reason;
            Exception = exception;
            Result = default;
            IsSuccess = false;
        }
    }
}