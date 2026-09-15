using System;
using SchoolPortal.Application.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class WalletAdjustmentResultViewModel
    {
        public WalletAdjustmentResultViewModel()
        {
        }

        public WalletAdjustmentResultViewModel(WalletAdjustmentResult result)
        {
            AdjustmentId = result.AdjustmentId;
            UserId = result.UserId;
            Amount = result.Amount;
            BalanceAfter = result.BalanceAfter;
            OccurredAt = result.OccurredAt;
            WasReplayed = result.WasReplayed;
        }

        public Guid AdjustmentId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public bool WasReplayed { get; set; }
    }
}
