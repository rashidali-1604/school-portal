using System;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users
{
    public class WalletAdjustmentDto
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public AdjustmentReason Reason { get; set; }

        public string Note { get; set; }

        public string PerformedBy { get; set; }

        public DateTimeOffset OccurredAt { get; set; }
    }
}
