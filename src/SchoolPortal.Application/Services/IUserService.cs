using System;
using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Services
{
    public interface IUserService : IService<User>
    {
        Task<PagedResult<UserListItemDto>> SearchAsync(UserSearchCriteria criteria, CancellationToken cancellationToken);

        Task<Result<UserDetailsDto>> GetDetailsAsync(Guid id, CancellationToken cancellationToken);

        Task<Result<WalletAdjustmentResult>> AdjustWalletAsync(WalletAdjustmentRequest request, CancellationToken cancellationToken);
    }
}
