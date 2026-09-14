using System;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users;

public sealed class UserListItemDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public UserRole Role { get; init; }
    public UserStatus Status { get; init; }
    public decimal WalletBalance { get; init; }
}
