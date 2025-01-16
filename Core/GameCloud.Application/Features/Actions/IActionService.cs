using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;

namespace GameCloud.Application.Features.Actions;

public interface IActionService
{
    Task<ActionResponse> ExecuteActionAsync(Guid sessionId, Guid userId, ActionRequest request, bool isTest = false);
    Task<IEnumerable<ActionResponse>> GetActionsBySessionAsync(Guid sessionId);

    Task<ActionStatsResponse> GetFunctionStatsAsync(
        Guid functionId,
        DateTimeRange range);

    Task<ActionListStatsResponse> GetGameFunctionsStatsAsync(
        Guid gameId,
        DateTimeRange range);

    Task<PageableListResponse<ActionResponse>> GetTestedFunctionLogs(Guid functionId, PageableRequest request);
    Task<ActionResponse> GetFunctionLogByActionId(Guid actionId);
}