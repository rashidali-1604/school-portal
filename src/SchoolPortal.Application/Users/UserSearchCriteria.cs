using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users;

public sealed class UserSearchCriteria
{
    public string SearchTerm { get; init; }
    public UserRole? Role { get; init; }
    public UserStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
