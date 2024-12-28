using System.Text.Json;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Features.Functions;

public interface IFunctionService
{
    Task<FunctionResponse> CreateFunctionAsync(Guid gameId, FunctionRequest request, Guid userId);
    Task<PageableListResponse<FunctionResponse>> GetFunctionsAsync(Guid gameId, PageableRequest request);
}