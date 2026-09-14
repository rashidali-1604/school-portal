using System;

namespace SchoolPortal.Application.Users.Commands;

public sealed class WalletAdjustmentResultDto
{
    public Guid AdjustmentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public bool WasReplayed { get; init; }
}
