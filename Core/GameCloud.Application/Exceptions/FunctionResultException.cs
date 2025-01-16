using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Exceptions;

public class FunctionResultException : Exception
{
    public FunctionStatus Status { get; }

    public FunctionError? Error { get; }

    public FunctionResultException(FunctionResult result)
        : base(GenerateMessage(result))
    {
        Status = result.Status;
        Error = result.Error;
    }

    private static string GenerateMessage(FunctionResult result)
    {
        var errorMessage = result.Error?.Message ?? "No error details available.";
        return $"FunctionResult indicates a failure with status '{result.Status}': {errorMessage}";
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