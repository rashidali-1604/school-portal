using System.ComponentModel.DataAnnotations;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.Contracts;

public sealed class WalletAdjustmentRequest
{
    [Required]
    [Range(typeof(decimal), "-1000000", "1000000", ErrorMessage = "Amount must be within +/-1,000,000.")]
    public decimal Amount { get; set; }

    [Required]
    public AdjustmentReason Reason { get; set; }

    [MaxLength(512)]
    public string Note { get; set; }
}
