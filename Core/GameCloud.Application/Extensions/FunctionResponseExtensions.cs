using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Extensions;

public static class FunctionResponseExtensions
{
    public static object ToEvent(this FunctionResult functionResult)
    {
        return new
        {
            FunctionId = functionResult.Id,
            Changes = functionResult.EntityUpdates,
            Status = functionResult.Status
        };
    }
}