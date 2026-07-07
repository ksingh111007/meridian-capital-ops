using Meridian.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Middleware;

/// <summary>Maps <see cref="DomainException"/> to RFC 7807 ProblemDetails responses.</summary>
public sealed class DomainExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception) when (!context.Response.HasStarted)
        {
            var status = exception.Kind switch
            {
                ErrorKind.Validation => StatusCodes.Status400BadRequest,
                ErrorKind.NotFound => StatusCodes.Status404NotFound,
                ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
                ErrorKind.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError,
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = exception.Kind.ToString(),
                Detail = exception.Message,
                Instance = context.Request.Path,
            });
        }
    }
}
