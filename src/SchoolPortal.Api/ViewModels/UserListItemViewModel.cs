using System;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class UserListItemViewModel
    {
        public UserListItemViewModel()
        {
        }

        public UserListItemViewModel(UserListItemDto dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            Email = dto.Email;
            Role = dto.Role;
            Status = dto.Status;
            WalletBalance = dto.WalletBalance;
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public decimal WalletBalance { get; set; }
    }
}
