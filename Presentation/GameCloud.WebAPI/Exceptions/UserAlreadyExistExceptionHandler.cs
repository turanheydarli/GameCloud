using GameCloud.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Exceptions;

public class UserAlreadyExistExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorResponse = new ProblemDetails();

        if (exception is not UserAlreadyExistsException userAlreadyExistsException)
        {
            return false;
        }

        errorResponse.Type = exception.GetType().Name;
        errorResponse.Detail = exception.Message;
        errorResponse.Status = StatusCodes.Status409Conflict;
        errorResponse.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        errorResponse.Extensions = new Dictionary<string, object>()
        {
            { "requestId", httpContext.TraceIdentifier }
        }!;

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status409Conflict;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = errorResponse,
        });

        return true;
    }
}