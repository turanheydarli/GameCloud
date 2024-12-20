using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.WebAPI.Exceptions;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorResponse = new ProblemDetails();

        switch (exception)
        {
            case BadHttpRequestException:
                errorResponse.Status = (int)HttpStatusCode.BadRequest;

                errorResponse.Type = exception.GetType().Name;
                errorResponse.Detail = exception.Message;
                errorResponse.Instance = $"{exception.GetType().Name}: {exception.Message}";
                errorResponse.Extensions = new Dictionary<string, object>()
                {
                    { "requestId", httpContext.TraceIdentifier }
                }!;

                break;

            default:
                errorResponse.Status = (int)HttpStatusCode.InternalServerError;

                errorResponse.Type = exception.GetType().Name;
                errorResponse.Detail = exception.Message;
                errorResponse.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
                errorResponse.Extensions = new Dictionary<string, object>()
                {
                    { "requestId", httpContext.TraceIdentifier }
                }!;

                break;
        }

        httpContext.Response.StatusCode = errorResponse.Status ?? StatusCodes.Status500InternalServerError;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext()
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = errorResponse,
        });

        return true;
    }
}