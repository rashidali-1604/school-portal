using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Application.Common;
using SchoolPortal.Domain.Common;

namespace SchoolPortal.Application.Users.Commands;

public sealed class AdjustWalletHandler
{
    private readonly IUserRepository _users;
    private readonly IClock _clock;
    private readonly ILogger<AdjustWalletHandler> _logger;

    public AdjustWalletHandler(
        IUserRepository users,
        IClock clock,
        ILogger<AdjustWalletHandler> logger)
    {
        _users = users;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<WalletAdjustmentResultDto>> HandleAsync(
        AdjustWalletCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Amount == 0m)
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.Validation("wallet.amount_zero", "Adjustment amount cannot be zero."));
        }

        if (string.IsNullOrWhiteSpace(command.RequestId))
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.Validation("wallet.request_id_required", "Idempotency key is required."));
        }

        var existing = await _users.FindAdjustmentByRequestIdAsync(command.UserId, command.RequestId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation(
                "Wallet adjustment replay detected for RequestId={RequestId} UserId={UserId}",
                command.RequestId, command.UserId);
            return Result<WalletAdjustmentResultDto>.Success(new WalletAdjustmentResultDto
            {
                AdjustmentId = existing.Id,
                UserId = existing.UserId,
                Amount = existing.Amount,
                BalanceAfter = existing.BalanceAfter,
                OccurredAt = existing.OccurredAt,
                WasReplayed = true
            });
        }

        var user = await _users.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.NotFound("user.not_found", $"User {command.UserId} was not found."));
        }

        try
        {
            var adjustment = user.AdjustWallet(
                command.Amount,
                command.Reason,
                command.Note,
                command.RequestId,
                command.PerformedBy,
                _clock.UtcNow);

            await _users.SaveChangesAsync(cancellationToken);

            return Result<WalletAdjustmentResultDto>.Success(new WalletAdjustmentResultDto
            {
                AdjustmentId = adjustment.Id,
                UserId = user.Id,
                Amount = adjustment.Amount,
                BalanceAfter = adjustment.BalanceAfter,
                OccurredAt = adjustment.OccurredAt,
                WasReplayed = false
            });
        }
        catch (InsufficientFundsException ex)
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.Domain("wallet.insufficient_funds", ex.Message));
        }
        catch (UserNotActiveException ex)
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.Conflict("user.not_active", ex.Message));
        }
        catch (DomainException ex)
        {
            return Result<WalletAdjustmentResultDto>.Failure(
                Error.Domain("wallet.domain_error", ex.Message));
        }
    }
}
