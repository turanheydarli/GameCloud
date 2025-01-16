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

        if (fre.Status == FunctionStatus.Success)
        {
            return false;
        }

        var statusCode = fre.Status switch
        {
            FunctionStatus.PartialSuccess => StatusCodes.Status206PartialContent,
            FunctionStatus.Pending        => StatusCodes.Status202Accepted,
            _                             => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Type     = exception.GetType().Name, 
            Detail   = exception.Message,
            Status   = statusCode,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            Extensions =
            {
                ["requestId"] = httpContext.TraceIdentifier,
                ["functionStatus"] = fre.Status.ToString()
            }
        };

        if (!string.IsNullOrEmpty(fre.Error?.Code))
        {
            problemDetails.Extensions["errorCode"] = fre.Error.Code;
        }

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext    = httpContext,
            Exception      = exception,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
