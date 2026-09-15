using System;
using System.Collections.Generic;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public UserRole Role { get; set; }

        public UserStatus Status { get; set; }

        public decimal WalletBalance { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public IList<WalletAdjustmentDto> RecentAdjustments { get; set; } = new List<WalletAdjustmentDto>();
    }
}
