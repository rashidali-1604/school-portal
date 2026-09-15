using System;
using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<PagedResult<UserListItemDto>> SearchAsync(UserSearchCriteria criteria, CancellationToken cancellationToken);

        Task<UserDetailsDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken);

        Task<WalletAdjustment> FindAdjustmentByRequestIdAsync(Guid userId, string requestId, CancellationToken cancellationToken);
    }
}
