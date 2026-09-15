using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Interfaces;
using SchoolPortal.Application.Repositories;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Common;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Application.Services
{
    public class UserService : Service<User>, IUserService
    {
        private readonly IUserRepository _users;
        private readonly IClock _clock;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository users, IClock clock, ILogger<UserService> logger)
            : base(users)
        {
            _users = users;
            _clock = clock;
            _logger = logger;
        }

        public Task<PagedResult<UserListItemDto>> SearchAsync(UserSearchCriteria criteria, CancellationToken cancellationToken)
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

        public async Task<Result<UserDetailsDto>> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
        {
            var details = await _users.GetDetailsAsync(id, cancellationToken);
            if (details is null)
            {
                return Result<UserDetailsDto>.Failure(Error.NotFound("user.not_found", $"User {id} was not found."));
            }
            return Result<UserDetailsDto>.Success(details);
        }

        public async Task<Result<WalletAdjustmentResult>> AdjustWalletAsync(WalletAdjustmentRequest request, CancellationToken cancellationToken)
        {
            if (request.Amount == 0m)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Validation("wallet.amount_zero", "Adjustment amount cannot be zero."));
            }

            if (string.IsNullOrWhiteSpace(request.RequestId))
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Validation("wallet.request_id_required", "Idempotency key is required."));
            }

            var replay = await _users.FindAdjustmentByRequestIdAsync(request.UserId, request.RequestId, cancellationToken);
            if (replay is not null)
            {
                _logger.LogInformation("Wallet adjustment replay for RequestId={RequestId} UserId={UserId}", request.RequestId, request.UserId);
                return Result<WalletAdjustmentResult>.Success(new WalletAdjustmentResult
                {
                    AdjustmentId = replay.Id,
                    UserId = replay.UserId,
                    Amount = replay.Amount,
                    BalanceAfter = replay.BalanceAfter,
                    OccurredAt = replay.OccurredAt,
                    WasReplayed = true
                });
            }

            var user = await _users.FindAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.NotFound("user.not_found", $"User {request.UserId} was not found."));
            }

            WalletAdjustment adjustment;
            try
            {
                adjustment = user.AdjustWallet(request.Amount, request.Reason, request.Note, request.RequestId, request.PerformedBy, _clock.UtcNow);
            }
            catch (InsufficientFundsException ex)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Domain("wallet.insufficient_funds", ex.Message));
            }
            catch (UserNotActiveException ex)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Conflict("user.not_active", ex.Message));
            }
            catch (DomainException ex)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Domain("wallet.domain_error", ex.Message));
            }

            var saveResult = await _users.SaveAllAsync(cancellationToken);
            if (!saveResult.IsSuccessful)
            {
                return Result<WalletAdjustmentResult>.Failure(Error.Conflict("wallet.save_failed", saveResult.Message));
            }

            return Result<WalletAdjustmentResult>.Success(new WalletAdjustmentResult
            {
                AdjustmentId = adjustment.Id,
                UserId = user.Id,
                Amount = adjustment.Amount,
                BalanceAfter = adjustment.BalanceAfter,
                OccurredAt = adjustment.OccurredAt,
                WasReplayed = false
            });
        }
    }
}
