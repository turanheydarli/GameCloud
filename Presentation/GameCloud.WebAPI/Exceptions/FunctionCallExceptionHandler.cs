using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Domain.Enums;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Exceptions;

public class FunctionCallExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FunctionResultException fre)
        {
            return false;
        }

        if (!fre.IsSuccess)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type     = exception.GetType().Name, 
            Detail   = exception.Message,
            Status   = StatusCodes.Status400BadRequest,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Extensions =
            {
                ["requestId"] = httpContext.TraceIdentifier,
                ["functionStatus"] = fre.IsSuccess.ToString()
            }
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext    = httpContext,
            Exception      = exception,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
