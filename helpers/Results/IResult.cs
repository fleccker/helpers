namespace helpers.Results
{
    public interface IResult<TResult>
    {
        bool IsSuccess { get; }

        TResult Result { get; }
    }
}