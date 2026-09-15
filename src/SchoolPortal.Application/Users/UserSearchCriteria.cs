using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Users
{
    public class UserSearchCriteria
    {
        public string SearchTerm { get; set; }

        public UserRole? Role { get; set; }

        public UserStatus? Status { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
