namespace Uber.Domain.Common;

public class Result
{
    private protected Result(Error? error)
    {
        Error = error;
    }

    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }
}
