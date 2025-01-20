using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Domain.Dynamics;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class ActionService(
    IActionLogRepository actionLogRepository,
    IFunctionRepository functionRepository,
    IFunctionExecutor functionExecutor,
    IPlayerService playerService,
    INotificationService notificationService,
    IGameContext gameContext,
    IMapper mapper,
    IEventPublisher eventPublisher,
    ExecutionContextAccessor executionContextAccessor)
    : IActionService
{
    public async Task<ActionResponse> ExecuteActionAsync(
        Guid sessionId,
        Guid userId,
        ActionRequest request,
        bool isTest = false)
    {
        var startTime = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();
        var retryCount = 0;
        var actionLog = new ActionLog
        {
            IsTestMode = isTest,
            CreatedAt = startTime,
            StartedAt = startTime,
            SessionId = sessionId,
            ActionType = request.ActionType,
            UserId = userId,
            Payload = request.Payload.Deserialize<JsonDocument>(),
            PayloadSizeBytes = System.Text.Encoding.UTF8.GetBytes(
                request.Payload.ToString()).Length
        };

        try
        {
            executionContextAccessor.SetContext(ActionExecutionContext.Function);

            FunctionConfig functionConfig = await functionRepository.GetByActionTypeAsync(request.ActionType);

            if (functionConfig == null)
            {
                throw new ApplicationException($"Action type '{request.ActionType}' not found.");
            }

            actionLog.FunctionId = functionConfig.Id;

            if (!functionConfig.IsEnabled)
            {
                throw new ApplicationException($"Function '{functionConfig.Name}' is not enabled.");
            }

            var invokeRequest = new FunctionInvokeRequest(
                Endpoint: functionConfig.Endpoint,
                SessionId: sessionId,
                Payload: request.Payload
            );

            var executionSw = Stopwatch.StartNew();
            FunctionResult? functionResult = await functionExecutor.InvokeAsync(invokeRequest);
            executionSw.Stop();

            if (functionResult == null)
            {
                throw new ApplicationException("FunctionExecutor returned null result unexpectedly.");
            }

            if (functionResult.EntityUpdates is not null)
            {
                foreach (var (entityId, attributeUpdates) in functionResult.EntityUpdates)
                {
                    // await eventPublisher.PublishAsync(new AttributeUpdateEvent(
                    //     userId,
                    //     entityId,
                    //     attributeUpdates));

                    await playerService.ApplyAttributeUpdatesAsync(entityId, attributeUpdates);
                }
            }

            if (functionResult.Notifications is not null)
            {
                await notificationService.RegisterNotificationList(functionResult.Notifications);
            }

            actionLog.Status = functionResult.Status;
            actionLog.ErrorCode = functionResult.Error?.Code;
            actionLog.ErrorMessage = functionResult.Error?.Message;
            actionLog.ExecutionTimeMs = executionSw.ElapsedMilliseconds;
            actionLog.Result = JsonSerializer.SerializeToDocument(functionResult);
            actionLog.ResultSizeBytes = System.Text.Encoding.UTF8.GetBytes(
                functionResult.ToString()).Length;
            actionLog.RetryCount = retryCount;
            actionLog.CompletedAt = DateTime.UtcNow;
            actionLog.TotalLatencyMs = sw.ElapsedMilliseconds;
            actionLog.Metadata = new Dictionary<string, string>
            {
                ["functionVersion"] = functionConfig.Version,
                ["clientVersion"] = request.ClientVersion,
                ["clientPlatform"] = request.ClientPlatform
            };

            await actionLogRepository.CreateAsync(actionLog);

            if (functionResult.Status != FunctionStatus.Success)
            {
                throw new ApplicationException($"Action failed: {functionResult.Error?.Message}");
            }

            return mapper.Map<ActionResponse>(actionLog);
        }
        catch (Exception ex)
        {
            actionLog.Status = FunctionStatus.Failed;
            actionLog.ErrorMessage = ex.Message;
            actionLog.ErrorCode = ex.GetType().Name;
            actionLog.CompletedAt = DateTime.UtcNow;
            actionLog.TotalLatencyMs = sw.ElapsedMilliseconds;

            await actionLogRepository.CreateAsync(actionLog);
            throw;
        }
        finally
        {
            executionContextAccessor.SetContext(ActionExecutionContext.Api);
        }
    }

    public async Task<IEnumerable<ActionResponse>> GetActionsBySessionAsync(Guid sessionId)
    {
        var logs = await actionLogRepository.GetBySessionAsync(sessionId);
        return logs.Items.Select(mapper.Map<ActionResponse>);
    }

    public async Task<ActionStatsResponse> GetFunctionStatsAsync(
        Guid functionId,
        DateTimeRange range)
    {
        var actions = await actionLogRepository.GetListActionByFunctionAsync(
            functionId,
            range.From,
            range.To);

        var totalCount = actions.Count();
        var successCount = actions.Count(a => a.IsSuccess());
        var errorCount = totalCount - successCount;

        var topErrors = actions
            .Where(a => !a.IsSuccess())
            .GroupBy(a => new { a.ErrorCode, a.ErrorMessage })
            .Select(g => new ErrorStat(
                g.Key.ErrorCode ?? "Unknown",
                g.Key.ErrorMessage ?? "Unknown error",
                g.Count(),
                g.Max(a => a.ExecutedAt),
                errorCount > 0 ? (double)g.Count() / errorCount * 100 : 0
            ))
            .OrderByDescending(e => e.Count)
            .Take(5)
            .ToList();

        var latencies = actions
            .Select(a => a.TotalLatencyMs)
            .OrderBy(l => l)
            .ToList();

        return new ActionStatsResponse(
            FunctionId: functionId,
            ActionType: actions.FirstOrDefault()?.ActionType ?? string.Empty,
            TotalExecutions: totalCount,
            SuccessfulExecutions: successCount,
            FailedExecutions: errorCount,
            SuccessRate: totalCount > 0 ? (double)successCount / totalCount * 100 : 0,
            AverageExecutionTimeMs: actions.Any() ? actions.Average(a => a.ExecutionTimeMs) : 0,
            AverageLatencyMs: actions.Any() ? actions.Average(a => a.TotalLatencyMs) : 0,
            P95LatencyMs: CalculatePercentile(latencies, 95),
            P99LatencyMs: CalculatePercentile(latencies, 99),
            TotalPayloadBytes: actions.Any() ? actions.Sum(a => a.PayloadSizeBytes) : 0,
            TotalResultBytes: actions.Any() ? actions.Sum(a => a.ResultSizeBytes) : 0,
            AveragePayloadSizeBytes: actions.Any() ? actions.Average(a => a.PayloadSizeBytes) : 0,
            AverageResultSizeBytes: actions.Any() ? actions.Average(a => a.ResultSizeBytes) : 0,
            LastExecutedAt: actions.MaxBy(a => a.ExecutedAt)?.ExecutedAt,
            LastErrorCode: actions.Where(a => !a.IsSuccess()).MaxBy(a => a.ExecutedAt)?.ErrorCode,
            LastErrorMessage: actions.Where(a => !a.IsSuccess()).MaxBy(a => a.ExecutedAt)?.ErrorMessage,
            TopErrors: topErrors,
            WindowStart: range.From,
            WindowEnd: range.To
        );
    }

    public async Task<ActionListStatsResponse> GetGameFunctionsStatsAsync(
        Guid gameId,
        DateTimeRange range)
    {
        var functions = await functionRepository.GetListAsync(gameId);
        var functionStats = new List<ActionStatsResponse>();

        foreach (var function in functions)
        {
            var stats = await GetFunctionStatsAsync(function.Id, range);
            functionStats.Add(stats);
        }

        var totalExecutions = functionStats.Sum(s => s.TotalExecutions);
        var totalSuccesses = functionStats.Sum(s => s.SuccessfulExecutions);

        var statsWithExecutions = functionStats.Where(s => s.TotalExecutions > 0).ToList();

        var averageExecutionTimeMs = statsWithExecutions.Any()
            ? statsWithExecutions.Average(s => s.AverageExecutionTimeMs)
            : 0;

        var averageLatencyMs = statsWithExecutions.Any()
            ? statsWithExecutions.Average(s => s.AverageLatencyMs)
            : 0;

        return new ActionListStatsResponse(
            GameId: gameId,
            TotalFunctions: functions.Count,
            ActiveFunctions: functions.Count(f => f.IsEnabled),
            TotalExecutions: totalExecutions,
            OverallSuccessRate: totalExecutions > 0
                ? (double)totalSuccesses / totalExecutions * 100
                : 0,
            AverageExecutionTimeMs: averageExecutionTimeMs,
            AverageLatencyMs: averageLatencyMs,
            TotalInboundTrafficMB: functionStats.Any()
                ? functionStats.Sum(s => s.TotalPayloadBytes) / 1024.0 / 1024.0
                : 0,
            TotalOutboundTrafficMB: functionStats.Any()
                ? functionStats.Sum(s => s.TotalResultBytes) / 1024.0 / 1024.0
                : 0,
            FunctionStats: functionStats,
            WindowStart: range.From,
            WindowEnd: range.To
        );
    }

    public async Task<PageableListResponse<ActionResponse>> GetAllPagedDynamicFunctionLogs(Guid functionId,
        DynamicRequest request)
    {
        var logs = await actionLogRepository.GetPagedDynamicFunctionLogs(functionId, request);

        return mapper.Map<PageableListResponse<ActionResponse>>(logs);
    }

    public async Task<ActionResponse> GetFunctionLogByActionId(Guid actionId)
    {
        var actionLog = await actionLogRepository.GetByIdAsync(actionId);

        if (actionLog == null)
            throw new NotFoundException("ActionLog", actionId);

        return mapper.Map<ActionResponse>(actionLog);
    }


    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (!values.Any()) return 0;

        var index = (int)Math.Ceiling((percentile / 100.0) * values.Count) - 1;
        return values[Math.Max(0, Math.Min(index, values.Count - 1))];
    }
}