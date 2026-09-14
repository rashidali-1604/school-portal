using System;
using System.Collections.Generic;
using SchoolPortal.Domain.Common;

namespace SchoolPortal.Domain.Users;

public class User
{
    private readonly List<WalletAdjustment> _adjustments = new();

    private User()
    {
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Email { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public decimal WalletBalance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyCollection<WalletAdjustment> Adjustments => _adjustments.AsReadOnly();

    public static User Register(
        string firstName,
        string lastName,
        string email,
        UserRole role,
        DateTimeOffset now,
        decimal openingBalance = 0m)
    {
        RequireName(firstName, nameof(firstName));
        RequireName(lastName, nameof(lastName));
        RequireEmail(email);
        if (openingBalance < 0m)
        {
            throw new DomainException("Opening balance cannot be negative.");
        }

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Status = UserStatus.Active,
            WalletBalance = openingBalance,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public WalletAdjustment AdjustWallet(
        decimal amount,
        AdjustmentReason reason,
        string note,
        string requestId,
        string performedBy,
        DateTimeOffset now)
    {
        if (amount == 0m)
        {
            throw new DomainException("Adjustment amount cannot be zero.");
        }

        if (Status != UserStatus.Active)
        {
            throw new UserNotActiveException(Id);
        }

        var newBalance = WalletBalance + amount;
        if (newBalance < 0m)
        {
            throw new InsufficientFundsException(WalletBalance, amount);
        }

        var adjustment = WalletAdjustment.Record(
            Id,
            amount,
            newBalance,
            reason,
            note,
            requestId,
            performedBy,
            now);

        _adjustments.Add(adjustment);
        WalletBalance = newBalance;
        UpdatedAt = now;
        Version++;

        return adjustment;
    }

    private static void RequireName(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{field} is required.");
        }
    }

    private static void RequireEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
        {
            throw new DomainException("A valid email is required.");
        }
    }
}
