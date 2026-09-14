using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolPortal.Application.Common;

namespace SchoolPortal.Api.Http;

internal static class ProblemDetailsFactoryExtensions
{
    public static ObjectResult ToActionResult(this Error error)
    {
        var (status, title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Invalid request"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Domain => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            ErrorType.Concurrency => (StatusCodes.Status412PreconditionFailed, "Concurrent modification"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Message,
            Type = $"about:blank#{error.Code}"
        };
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }
}
