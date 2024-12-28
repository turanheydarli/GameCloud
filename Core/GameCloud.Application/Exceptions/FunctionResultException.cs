using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Exceptions
{
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
}