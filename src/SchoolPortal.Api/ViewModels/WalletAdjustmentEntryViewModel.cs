using System;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class WalletAdjustmentEntryViewModel
    {
        public WalletAdjustmentEntryViewModel()
        {
        }

        public WalletAdjustmentEntryViewModel(WalletAdjustmentDto dto)
        {
            Id = dto.Id;
            Amount = dto.Amount;
            BalanceAfter = dto.BalanceAfter;
            Reason = dto.Reason;
            Note = dto.Note;
            PerformedBy = dto.PerformedBy;
            OccurredAt = dto.OccurredAt;
        }

        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public AdjustmentReason Reason { get; set; }
        public string Note { get; set; }
        public string PerformedBy { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
    }
}
