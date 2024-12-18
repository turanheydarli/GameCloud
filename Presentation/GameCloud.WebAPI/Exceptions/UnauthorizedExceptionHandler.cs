using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Exceptions;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorResponse = new ProblemDetails();

        if (exception is not UnauthorizedAccessException unauthorizedException)
        {
            return false;
        }

        errorResponse.Status = (int)HttpStatusCode.Unauthorized;
        errorResponse.Title = exception.GetType().Name;

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}