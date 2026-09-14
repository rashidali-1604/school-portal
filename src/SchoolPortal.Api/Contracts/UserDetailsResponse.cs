using System;
using System.Collections.Generic;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.Contracts;

public sealed class UserDetailsResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public UserRole Role { get; init; }
    public UserStatus Status { get; init; }
    public decimal WalletBalance { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<WalletAdjustmentResponse> RecentAdjustments { get; init; }
}

public sealed class WalletAdjustmentResponse
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public AdjustmentReason Reason { get; init; }
    public string Note { get; init; }
    public string PerformedBy { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
