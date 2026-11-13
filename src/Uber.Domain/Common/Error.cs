namespace Uber.Domain.Common;

public sealed record Error
{
    public Error(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }
}
