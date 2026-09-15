using System;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users
{
    public class WalletAdjustmentRequest
    {
        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public AdjustmentReason Reason { get; set; }

        public string Note { get; set; }

        public string RequestId { get; set; }

        public string PerformedBy { get; set; }
    }
}
