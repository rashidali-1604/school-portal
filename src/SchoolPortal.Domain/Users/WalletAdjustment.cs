using System;

namespace SchoolPortal.Domain.Users;

public class WalletAdjustment
{
    private WalletAdjustment()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal Amount { get; private set; }

    public decimal BalanceAfter { get; private set; }

    public AdjustmentReason Reason { get; private set; }

    public string Note { get; private set; }

    public string RequestId { get; private set; }

    public string PerformedBy { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    internal static WalletAdjustment Record(
        Guid userId,
        decimal amount,
        decimal balanceAfter,
        AdjustmentReason reason,
        string note,
        string requestId,
        string performedBy,
        DateTimeOffset occurredAt)
    {
        return new WalletAdjustment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Reason = reason,
            Note = note,
            RequestId = requestId,
            PerformedBy = performedBy,
            OccurredAt = occurredAt
        };
    }
}
