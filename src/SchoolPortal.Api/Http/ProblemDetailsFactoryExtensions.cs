using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolPortal.Application.Common;

namespace SchoolPortal.Api.Http
{
    internal static class ProblemDetailsFactoryExtensions
    {
        public static ObjectResult ToActionResult(this Error error)
        {
            int status;
            string title;

            switch (error.Type)
            {
                case ErrorType.NotFound:
                    status = StatusCodes.Status404NotFound;
                    title = "Resource not found";
                    break;
                case ErrorType.Validation:
                    status = StatusCodes.Status400BadRequest;
                    title = "Invalid request";
                    break;
                case ErrorType.Conflict:
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    break;
                case ErrorType.Domain:
                    status = StatusCodes.Status422UnprocessableEntity;
                    title = "Business rule violated";
                    break;
                case ErrorType.Concurrency:
                    status = StatusCodes.Status412PreconditionFailed;
                    title = "Concurrent modification";
                    break;
                default:
                    status = StatusCodes.Status500InternalServerError;
                    title = "Unexpected error";
                    break;
            }

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
}
