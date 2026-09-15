using System;

namespace SchoolPortal.Application.Users
{
    public class WalletAdjustmentResult
    {
        public Guid AdjustmentId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTimeOffset OccurredAt { get; set; }

        public bool WasReplayed { get; set; }
    }
}
