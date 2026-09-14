using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Application.Common;

namespace SchoolPortal.Application.Users.Queries;

public sealed class SearchUsersHandler
{
    private readonly IUserRepository _users;

    public SearchUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public Task<PagedResult<UserListItemDto>> HandleAsync(UserSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var normalised = new UserSearchCriteria
        {
            SearchTerm = string.IsNullOrWhiteSpace(criteria.SearchTerm) ? null : criteria.SearchTerm.Trim(),
            Role = criteria.Role,
            Status = criteria.Status,
            Page = criteria.Page < 1 ? 1 : criteria.Page,
            PageSize = criteria.PageSize is < 1 or > 100 ? 20 : criteria.PageSize
        };

        return _users.SearchAsync(normalised, cancellationToken);
    }
}
