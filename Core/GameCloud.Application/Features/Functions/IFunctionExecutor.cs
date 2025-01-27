using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Functions;

public interface IFunctionExecutor
{
    Task<FunctionResult> InvokeAsync(FunctionInvokeRequest request);
}