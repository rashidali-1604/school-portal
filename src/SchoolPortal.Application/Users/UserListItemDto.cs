using System;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users
{
    public class UserListItemDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public UserRole Role { get; set; }

        public UserStatus Status { get; set; }

        public decimal WalletBalance { get; set; }
    }
}
