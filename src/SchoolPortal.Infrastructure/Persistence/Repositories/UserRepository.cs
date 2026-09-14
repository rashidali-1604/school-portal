using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Users;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly PortalDbContext _context;

    public UserRepository(PortalDbContext context)
    {
        _context = context;
    }

    public Task<User> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserDetailsDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDetailsDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Role = x.Role,
                Status = x.Status,
                WalletBalance = x.WalletBalance,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var recent = await _context.WalletAdjustments
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .OrderByDescending(x => x.OccurredAt)
            .Take(10)
            .Select(x => new WalletAdjustmentDto
            {
                Id = x.Id,
                Amount = x.Amount,
                BalanceAfter = x.BalanceAfter,
                Reason = x.Reason,
                Note = x.Note,
                PerformedBy = x.PerformedBy,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);

        return new UserDetailsDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            WalletBalance = user.WalletBalance,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            RecentAdjustments = recent
        };
    }

    public async Task<PagedResult<UserListItemDto>> SearchAsync(UserSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var term = criteria.SearchTerm.ToLowerInvariant();
            query = query.Where(x =>
                x.FirstName.ToLower().Contains(term)
                || x.LastName.ToLower().Contains(term)
                || x.Email.ToLower().Contains(term));
        }

        if (criteria.Role.HasValue)
        {
            var role = criteria.Role.Value;
            query = query.Where(x => x.Role == role);
        }

        if (criteria.Status.HasValue)
        {
            var status = criteria.Status.Value;
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new UserListItemDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Role = x.Role,
                Status = x.Status,
                WalletBalance = x.WalletBalance
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>(items, criteria.Page, criteria.PageSize, total);
    }

    public Task<WalletAdjustment> FindAdjustmentByRequestIdAsync(
        Guid userId,
        string requestId,
        CancellationToken cancellationToken)
    {
        return _context.WalletAdjustments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RequestId == requestId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
