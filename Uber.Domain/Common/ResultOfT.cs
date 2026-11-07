namespace Uber.Domain.Common;


public sealed class Result<T> : Result
{
    private readonly T _value;

    private Result(T value, Error? error) : base(error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(value, null);

    public new static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default!, error);
    }
}
