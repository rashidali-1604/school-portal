using System;

namespace SchoolPortal.Domain.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

public sealed class InsufficientFundsException : DomainException
{
    public InsufficientFundsException(decimal balance, decimal attempted)
        : base($"Wallet balance {balance:0.00} is insufficient for adjustment {attempted:0.00}.")
    {
        Balance = balance;
        Attempted = attempted;
    }

    public decimal Balance { get; }

    public decimal Attempted { get; }
}

public sealed class UserNotActiveException : DomainException
{
    public UserNotActiveException(Guid userId)
        : base($"User {userId} is not active and cannot receive wallet adjustments.")
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
