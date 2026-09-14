namespace SchoolPortal.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Domain,
    Concurrency
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Domain(string code, string message) => new(code, message, ErrorType.Domain);
    public static Error Concurrency(string code, string message) => new(code, message, ErrorType.Concurrency);
}
