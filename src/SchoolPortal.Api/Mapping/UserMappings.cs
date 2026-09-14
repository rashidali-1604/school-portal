using System.Linq;
using SchoolPortal.Api.Contracts;
using SchoolPortal.Application.Common;
using SchoolPortal.Application.Users;
using SchoolPortal.Application.Users.Commands;

namespace SchoolPortal.Api.Mapping;

internal static class UserMappings
{
    public static UserListItemResponse ToResponse(this UserListItemDto dto) => new()
    {
        Id = dto.Id,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Role = dto.Role,
        Status = dto.Status,
        WalletBalance = dto.WalletBalance
    };

    public static PagedResponse<UserListItemResponse> ToResponse(this PagedResult<UserListItemDto> paged) => new()
    {
        Items = paged.Items.Select(ToResponse).ToList(),
        Page = paged.Page,
        PageSize = paged.PageSize,
        TotalCount = paged.TotalCount,
        TotalPages = paged.TotalPages
    };

    public static UserDetailsResponse ToResponse(this UserDetailsDto dto) => new()
    {
        Id = dto.Id,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Role = dto.Role,
        Status = dto.Status,
        WalletBalance = dto.WalletBalance,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        RecentAdjustments = dto.RecentAdjustments.Select(a => new WalletAdjustmentResponse
        {
            Id = a.Id,
            Amount = a.Amount,
            BalanceAfter = a.BalanceAfter,
            Reason = a.Reason,
            Note = a.Note,
            PerformedBy = a.PerformedBy,
            OccurredAt = a.OccurredAt
        }).ToList()
    };

    public static WalletAdjustmentResponseModel ToResponse(this WalletAdjustmentResultDto dto) => new()
    {
        AdjustmentId = dto.AdjustmentId,
        UserId = dto.UserId,
        Amount = dto.Amount,
        BalanceAfter = dto.BalanceAfter,
        OccurredAt = dto.OccurredAt,
        WasReplayed = dto.WasReplayed
    };
}
