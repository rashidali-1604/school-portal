using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SchoolPortal.Api.Http;
using SchoolPortal.Api.Mapping;
using SchoolPortal.Application.Users;
using SchoolPortal.Application.Users.Commands;
using SchoolPortal.Application.Users.Queries;
using SchoolPortal.Domain.Users;

namespace SchoolPortal.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly SearchUsersHandler _search;
    private readonly GetUserByIdHandler _getById;
    private readonly AdjustWalletHandler _adjust;

    public UsersController(
        SearchUsersHandler search,
        GetUserByIdHandler getById,
        AdjustWalletHandler adjust)
    {
        _search = search;
        _getById = getById;
        _adjust = adjust;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "q")] string q,
        [FromQuery] UserRole? role,
        [FromQuery] UserStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var criteria = new UserSearchCriteria
        {
            SearchTerm = q,
            Role = role,
            Status = status,
            Page = page,
            PageSize = pageSize
        };
        var result = await _search.HandleAsync(criteria, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getById.HandleAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }
        return Ok(result.Value.ToResponse());
    }

    [HttpPost("{id:guid}/wallet-adjustments")]
    public async Task<IActionResult> Adjust(
        Guid id,
        [FromBody] Contracts.WalletAdjustmentRequest body,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new AdjustWalletCommand
        {
            UserId = id,
            Amount = body.Amount,
            Reason = body.Reason,
            Note = body.Note,
            RequestId = string.IsNullOrWhiteSpace(idempotencyKey) ? Guid.NewGuid().ToString("N") : idempotencyKey,
            PerformedBy = User?.Identity?.Name ?? "portal-admin"
        };

        var result = await _adjust.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        var response = result.Value.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.UserId }, response);
    }
}
