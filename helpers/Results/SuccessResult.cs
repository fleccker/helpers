namespace helpers.Results
{
    public struct SuccessResult : IResult<object>
    {
        public bool IsSuccess { get; }
        public object Result { get; }

        public SuccessResult(object result)
        {
            Result = result;
            IsSuccess = true;
        }
    }

    public struct SuccessResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess { get; }
        public TResult Result { get; }

        public SuccessResult(TResult result)
        {
            Result = result;
            IsSuccess = true;
        }
    }
}