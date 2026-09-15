using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SchoolPortal.Api.Http
{
    internal class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int status;
            string title;
            string detail;

            if (exception is DbUpdateConcurrencyException)
            {
                status = StatusCodes.Status412PreconditionFailed;
                title = "Concurrent modification";
                detail = "The resource was modified by another request. Refresh and try again.";
                _logger.LogWarning(exception, "Concurrency clash on {Path}", httpContext.Request.Path);
            }
            else
            {
                status = StatusCodes.Status500InternalServerError;
                title = "Unexpected error";
                detail = "An unexpected error occurred while processing the request.";
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            };

            httpContext.Response.StatusCode = status;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
