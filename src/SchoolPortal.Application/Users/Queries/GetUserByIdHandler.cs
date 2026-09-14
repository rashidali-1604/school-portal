using System;
using System.Threading;
using System.Threading.Tasks;
using SchoolPortal.Application.Abstractions;
using SchoolPortal.Application.Common;

namespace SchoolPortal.Application.Users.Queries;

public sealed class GetUserByIdHandler
{
    private readonly IUserRepository _users;

    public GetUserByIdHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<UserDetailsDto>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.GetDetailsAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<UserDetailsDto>.Failure(
                Error.NotFound("user.not_found", $"User {id} was not found."));
        }
        return Result<UserDetailsDto>.Success(user);
    }
}
