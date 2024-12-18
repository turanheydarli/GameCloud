using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Extensions;

public static class FunctionResponseExtensions
{
    public static object ToEvent(this FunctionResponse functionResponse)
    {
        return new
        {
            FunctionId = functionResponse.Id,
            Changes = functionResponse.Changes,
            Status = functionResponse.Status
        };
    }
}