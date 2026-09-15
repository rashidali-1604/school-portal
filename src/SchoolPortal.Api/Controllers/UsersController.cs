using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SchoolPortal.Api.Http;
using SchoolPortal.Api.ViewModels;
using SchoolPortal.Application.Services;

namespace SchoolPortal.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;

        public UsersController(IUserService users)
        {
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] UserSearchParametersViewModel parameters, CancellationToken cancellationToken)
        {
            var result = await _users.SearchAsync(parameters.ToCriteria(), cancellationToken);
            return Ok(PagedViewModel.From(result, x => new UserListItemViewModel(x)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _users.GetDetailsAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(new UserDetailsViewModel(result.Value)) : result.Error.ToActionResult();
        }

        [HttpPost("{id:guid}/wallet-adjustments")]
        public async Task<IActionResult> Adjust(Guid id, WalletAdjustmentViewModel body, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey, CancellationToken cancellationToken)
        {
            var request = body.ToRequest(id, idempotencyKey, User?.Identity?.Name);
            var result = await _users.AdjustWalletAsync(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return result.Error.ToActionResult();
            }
            var response = new WalletAdjustmentResultViewModel(result.Value);
            return CreatedAtAction(nameof(GetById), new { id = response.UserId }, response);
        }
    }
}
