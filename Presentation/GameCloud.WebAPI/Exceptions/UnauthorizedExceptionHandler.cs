using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace GameCloud.WebAPI.Exceptions;

public class UnauthorizedExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorResponse = new ProblemDetails();

        if (exception is not UnauthorizedAccessException unauthorizedException)
        {
            return false;
        }

        errorResponse.Type = exception.GetType().Name;
        errorResponse.Detail = exception.Message;
        errorResponse.Status = StatusCodes.Status401Unauthorized;
        errorResponse.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        errorResponse.Extensions = new Dictionary<string, object>()
        {
            { "requestId", httpContext.TraceIdentifier }
        }!;

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status401Unauthorized;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = errorResponse,
        });

        return true;
    }
}