using System;
using System.Collections.Generic;
using System.Linq;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class UserDetailsViewModel
    {
        public UserDetailsViewModel()
        {
        }

        public UserDetailsViewModel(UserDetailsDto dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            Email = dto.Email;
            Role = dto.Role;
            Status = dto.Status;
            WalletBalance = dto.WalletBalance;
            CreatedAt = dto.CreatedAt;
            UpdatedAt = dto.UpdatedAt;
            RecentAdjustments = dto.RecentAdjustments.Select(x => new WalletAdjustmentEntryViewModel(x)).ToList();
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public decimal WalletBalance { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public IList<WalletAdjustmentEntryViewModel> RecentAdjustments { get; set; } = new List<WalletAdjustmentEntryViewModel>();
    }
}
