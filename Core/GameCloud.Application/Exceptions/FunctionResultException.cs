using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Exceptions;

public class FunctionResultException : Exception
{
    public bool IsSuccess { get; set; }
    public string Error { get; }

    public FunctionResultException(FunctionResult result)
        : base(GenerateMessage(result))
    {
        IsSuccess = result.IsSuccess;
        Error = result.ErrorMessage;
    }

    private static string GenerateMessage(FunctionResult result)
    {
        var errorMessage = result.ErrorMessage ?? "No error details available.";
        return $"FunctionResult indicates a failure with status '{result.IsSuccess}': {errorMessage}";
    }
}

public class ImageNotFoundException : Exception
{
    public ImageNotFoundException(string message) : base(message)
    {
    }

    public ImageNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class ImageVariantNotFoundException : Exception
{
    public ImageVariantNotFoundException(string message) : base(message)
    {
    }
}public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}