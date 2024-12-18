using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;

namespace GameCloud.Application.Features.Actions;

public interface IActionService
{
    Task<ActionResponse> ExecuteActionAsync(Guid sessionId, ActionRequest request);
    Task<IEnumerable<ActionResponse>> GetActionsBySessionAsync(Guid sessionId);
}