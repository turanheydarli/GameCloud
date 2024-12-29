using System.Text.Json;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class ActionService(
    IActionLogRepository actionLogRepository,
    IFunctionRepository functionRepository,
    IFunctionExecutor functionExecutor,
    IPlayerService playerService,
    INotificationService notificationService,
    IGameContext gameContext)
    : IActionService
{
    public async Task<ActionResponse> ExecuteActionAsync(Guid sessionId, Guid userId, ActionRequest request)
    {
        FunctionConfig functionConfig = await functionRepository.GetByActionTypeAsync(request.ActionType);
        if (functionConfig == null)
        {
            throw new ApplicationException($"Action type '{request.ActionType}' not found.");
        }

        var invokeRequest = new FunctionInvokeRequest(
            Endpoint: functionConfig.Endpoint,
            SessionId: sessionId,
            Payload: request.Payload
        );

        FunctionResult? functionResult = await functionExecutor.InvokeAsync(invokeRequest);

        if (functionResult == null)
        {
            throw new ApplicationException("FunctionExecutor returned null result unexpectedly.");
        }

        if (functionResult.EntityUpdates is not null)
        {
            foreach (var (entityId, attributeUpdates) in functionResult.EntityUpdates)
            {
                await playerService.ApplyAttributeUpdatesAsync(entityId, attributeUpdates);
            }
        }

        if (functionResult.Notifications is not null)
        {
            await notificationService.RegisterNotificationList(functionResult.Notifications);
        }

        var actionLog = new ActionLog
        {
            CreatedAt = DateTime.UtcNow,
            ExecutedAt = DateTime.UtcNow,
            FunctionId = functionResult.Id,
            SessionId = sessionId,
            ActionType = request.ActionType,
            UserId = userId,
            Payload = request.Payload.Deserialize<JsonDocument>(),
            Result = JsonSerializer.SerializeToDocument(functionResult)
        };

        actionLog = await actionLogRepository.CreateAsync(actionLog);

        if (functionResult.Status != FunctionStatus.Success)
        {
            throw new ApplicationException($"Action failed: {functionResult.Error?.Message}");
        }

        return new ActionResponse(
            Id: actionLog.Id,
            SessionId: actionLog.SessionId,
            PlayerId: actionLog.UserId,
            ActionType: actionLog.ActionType,
            Payload: actionLog.Payload,
            Result: actionLog.Result,
            ExecutedAt: actionLog.ExecutedAt
        );
    }

    public async Task<IEnumerable<ActionResponse>> GetActionsBySessionAsync(Guid sessionId)
    {
        var logs = await actionLogRepository.GetBySessionAsync(sessionId);
        return logs.Items.Select(l => new ActionResponse(
            l.Id,
            l.SessionId,
            l.UserId,
            l.ActionType,
            l.Payload,
            l.Result,
            l.ExecutedAt
        ));
    }
}