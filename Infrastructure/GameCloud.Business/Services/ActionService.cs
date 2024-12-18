using System.Text.Json;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Extensions;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class ActionService(
    IActionLogRepository actionLogRepository,
    IEventPublisher eventPublisher,
    ISessionCache sessionCache,
    IFunctionService functionService,
    IFunctionRepository functionRepository,
    IGameContext gameContext)
    : IActionService
{
    public async Task<ActionResponse> ExecuteActionAsync(Guid sessionId, ActionRequest request)
    {
        FunctionConfig functionConfig = await functionRepository.GetByActionTypeAsync(request.ActionType);
        FunctionResponse functionResult = await functionService.InvokeAsync(functionConfig.Id, request.Parameters);

        var actionLog = await actionLogRepository.CreateAsync(new ActionLog
        {
            CreatedAt = DateTime.UtcNow,
            ExecutedAt = DateTime.UtcNow,
            FunctionId = functionResult.Id,
            Parameters = request.Parameters,
            Result = JsonSerializer.SerializeToDocument(functionResult)
        });

        await eventPublisher.PublishAsync(functionResult.ToEvent());

        await sessionCache.UpdateSessionStateAsync(request.SessionId, functionResult.Changes);

        return new ActionResponse(
            actionLog.Id,
            sessionId,
            actionLog.PlayerId,
            actionLog.ActionType,
            actionLog.Parameters,
            actionLog.Result,
            actionLog.ExecutedAt
        );
    }

    public async Task<IEnumerable<ActionResponse>> GetActionsBySessionAsync(Guid sessionId)
    {
        var logs = await actionLogRepository.GetBySessionAsync(sessionId);

        return logs.Select(l => new ActionResponse(
            l.Id,
            l.SessionId,
            l.PlayerId,
            l.ActionType,
            l.Parameters,
            l.Result,
            l.ExecutedAt));
    }
}