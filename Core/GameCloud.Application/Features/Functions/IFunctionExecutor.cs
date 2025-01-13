using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Features.Functions;

public interface IFunctionExecutor
{
    Task<FunctionResult?> InvokeAsync(FunctionInvokeRequest request);
}