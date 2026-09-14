using System;

namespace SchoolPortal.Application.Common;

public readonly struct Result<T>
{
    private readonly T _value;

    private Result(T value, Error error, bool isSuccess)
    {
        _value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public T Value
    {
        get
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Cannot access value of a failed result.");
            }
            return _value;
        }
    }

    public static Result<T> Success(T value) => new(value, default, true);

    public static Result<T> Failure(Error error) => new(default, error, false);
}
