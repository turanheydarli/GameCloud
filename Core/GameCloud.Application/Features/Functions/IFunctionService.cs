using System.Text.Json;
using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Features.Functions;

public interface IFunctionService
{
    Task<FunctionResponse> InvokeAsync(Guid id, JsonDocument parameters);
}