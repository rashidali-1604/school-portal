using System;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users.Commands;

public sealed class AdjustWalletCommand
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public AdjustmentReason Reason { get; init; }
    public string Note { get; init; }
    public string RequestId { get; init; }
    public string PerformedBy { get; init; }
}
