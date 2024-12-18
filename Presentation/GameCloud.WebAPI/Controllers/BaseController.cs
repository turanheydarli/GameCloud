using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Returns a standardized success response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="data">The response data.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>Standardized success response.</returns>
    protected IActionResult Success<T>(T data, string message = null)
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "Request successful.",
            Data = data
        });
    }

    /// <summary>
    /// Returns a standardized error response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>Standardized error response.</returns>
    protected IActionResult Error(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        });
    }
}

/// <summary>
/// A standardized API response model.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}