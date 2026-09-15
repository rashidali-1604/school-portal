using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.ViewModels
{
    public class UserSearchParametersViewModel
    {
        public string Q { get; set; }

        public UserRole? Role { get; set; }

        public UserStatus? Status { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public UserSearchCriteria ToCriteria()
        {
            return new UserSearchCriteria
            {
                SearchTerm = Q,
                Role = Role,
                Status = Status,
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}
