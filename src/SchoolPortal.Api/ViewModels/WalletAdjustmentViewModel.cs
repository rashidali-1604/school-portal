using System;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class WalletAdjustmentViewModel
    {
        public decimal Amount { get; set; }

        public AdjustmentReason Reason { get; set; }

        public string Note { get; set; }

        public WalletAdjustmentRequest ToRequest(Guid userId, string idempotencyKey, string performedBy)
        {
            return new WalletAdjustmentRequest
            {
                UserId = userId,
                Amount = Amount,
                Reason = Reason,
                Note = Note,
                RequestId = string.IsNullOrWhiteSpace(idempotencyKey) ? Guid.NewGuid().ToString("N") : idempotencyKey,
                PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "portal-admin" : performedBy
            };
        }
    }
}
