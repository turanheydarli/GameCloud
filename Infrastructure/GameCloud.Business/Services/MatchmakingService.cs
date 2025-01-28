using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace GameCloud.Business.Services;

public class MatchmakingService(
    IMatchmakingQueueRepository queueRepository,
    IMatchTicketRepository ticketRepository,
    IMatchRepository matchRepository,
    IMatchActionRepository actionRepository,
    IMapper mapper,
    IGameContext gameContext,
    IActionService actionService,
    IFunctionRepository functionRepository,
    ILogger<MatchmakingService> logger)
    : IMatchmakingService
{
    public async Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest request)
    {
        logger.LogInformation("Creating match queue {QueueName} for game {GameId}", request.Name, gameContext.GameId);

        var matchmaker = await functionRepository.GetByActionTypeAsync(
            gameContext.GameId,
            request.matchmakerFunctionName!);

        if (matchmaker == null)
        {
            logger.LogWarning("Matchmaker function {FunctionName} not found for game {GameId}",
                request.matchmakerFunctionName, gameContext.GameId);
            throw new MatchmakerFunctionNotFoundException(request.matchmakerFunctionName!, gameContext.GameId);
        }

        if (matchmaker == null)
        {
            throw new NotFoundException("The matchmaker was not found");
        }

        var queue = new MatchmakingQueue
        {
            GameId = gameContext.GameId,
            Name = request.Name,
            Description = request.Description,
            IsEnabled = true,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            TicketTTL = request.TicketTTL,
            Rules = request.Rules,
            MatchmakerFunctionId = matchmaker.Id
        };

        await queueRepository.CreateAsync(queue);

        logger.LogInformation("Created match queue {QueueId} ({QueueName})",
            queue.Id, queue.Name);

        return mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null,
        string? queueName = null)
    {
        MatchmakingQueue? queue = null;

        if (queueId.HasValue)
        {
            queue = await queueRepository.GetByIdAsync(queueId.Value);
        }
        else if (gameId.HasValue && !string.IsNullOrWhiteSpace(queueName))
        {
            queue = await queueRepository.GetByGameAndNameAsync(gameId.Value, queueName);
        }

        return queue == null ? null : mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task<MatchTicketResponse> CreateTicketAsync(
        Guid gameId,
        Guid playerId,
        string queueName,
        JsonDocument? properties = null)
    {
        logger.LogInformation("Creating match ticket for player {PlayerId} in queue {QueueName}",
            playerId, queueName);

        var queue = await queueRepository.GetByGameAndNameAsync(gameId, queueName);
        if (queue == null)
        {
            logger.LogWarning("Queue {QueueName} not found for game {GameId}",
                queueName, gameId);
            throw new MatchQueueNotFoundException(gameId, queueName);
        }

        var ticket = new MatchTicket
        {
            GameId = gameId,
            PlayerId = playerId,
            QueueName = queueName,
            Status = TicketStatus.Queued,
            Properties = properties,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(queue.TicketTTL)
        };

        await ticketRepository.CreateAsync(ticket);

        logger.LogInformation("Created match ticket {TicketId} for player {PlayerId}",
            ticket.Id, playerId);

        return mapper.Map<MatchTicketResponse>(ticket);
    }

    public async Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null)
    {
        logger.LogInformation("Starting matchmaking process {QueueId}",
            queueId?.ToString() ?? "for all queues");

        var createdMatches = new List<Match>();
        var queues = queueId.HasValue
            ? new[] { await queueRepository.GetByIdAsync(queueId.Value) }.Where(q => q != null)
            : await queueRepository.GetAllAsync();

        foreach (var queue in queues.Where(q => q.IsEnabled))
        {
            logger.LogDebug("Processing queue {QueueName} ({QueueId})", queue.Name, queue.Id);

            var activeTickets = await ticketRepository.GetActiveTicketsAsync(queue.Id);
            if (activeTickets.Count < queue.MinPlayers) continue;

            var matchedGroups = await FindMatchingPlayers(activeTickets, queue);
            foreach (var group in matchedGroups)
            {
                var initialState = await actionService.ExecuteActionAsync(
                    queue.Id,
                    queue.GameId,
                    new ActionRequest(queue.Id, "match.initialize",
                        JsonSerializer.SerializeToDocument(new
                        {
                            players = group.Select(t => t.Properties ?? JsonDocument.Parse("{}")),
                            rules = queue.Rules,
                            metadata = new
                            {
                                gameMode = queue.Rules.RootElement.GetProperty("gameMode").GetString() ?? "default",
                                version = "1.0"
                            }
                        })
                    )
                );

                if (!initialState.IsSuccess)
                    continue;

                var match = new Match
                {
                    GameId = queue.GameId,
                    QueueName = queue.Name,
                    State = MatchStatus.Ready,
                    CreatedAt = DateTime.UtcNow,
                    LastActionAt = DateTime.UtcNow,
                    PlayerIds = group.Select(t => t.PlayerId).ToList(),
                    PlayerStates = JsonSerializer.SerializeToDocument(
                        group.Select(t => t.Properties ?? JsonDocument.Parse("{}"))
                    ),
                    MatchState = initialState.Result.Data ?? JsonSerializer.SerializeToDocument(new MatchState
                    {
                        Status = MatchStateStatus.Joining,
                        Phase = MatchPhase.Initialization,
                        StartedAt = null,
                        GameState = queue.Rules.RootElement.TryGetProperty("initialState", out var initialStateValue)
                            ? JsonDocument.Parse(initialStateValue.ToString())
                            : JsonDocument.Parse("{}"),
                        Metadata = JsonDocument.Parse("{}"),
                        Presences = new List<PresenceState>()
                    }),
                    TurnHistory = JsonDocument.Parse("[]")
                };

                var matchState = match.MatchState.Deserialize<Dictionary<string, JsonElement>>();
                matchState["Presences"] = JsonSerializer.SerializeToElement(new List<Dictionary<string, object>>());
                match.MatchState = JsonSerializer.SerializeToDocument(matchState);

                await matchRepository.CreateAsync(match);
                createdMatches.Add(match);

                foreach (var ticket in group)
                {
                    ticket.Status = TicketStatus.Matched;
                    ticket.MatchId = match.Id;

                    await ticketRepository.UpdateAsync(ticket);
                }
            }

            logger.LogInformation("Created {MatchCount} matches from queue {QueueId}",
                createdMatches.Count, queue.Id);
        }

        return createdMatches.Select(m => mapper.Map<MatchResponse>(m)).ToList();
    }

    private async Task<List<List<MatchTicket>>> FindMatchingPlayers(List<MatchTicket> tickets, MatchmakingQueue queue)
    {
        if (queue.MatchmakerFunctionId.HasValue)
        {
            return await ExecuteCustomMatchmaker(tickets, queue);
        }

        return await ExecuteDefaultMatchmaker(tickets, queue);
    }

    private async Task<List<List<MatchTicket>>> ExecuteCustomMatchmaker(List<MatchTicket> tickets,
        MatchmakingQueue queue)
    {
        var ticketData = tickets.Select(t => new
        {
            id = t.Id,
            playerId = t.PlayerId,
            properties = t.Properties?.RootElement.ToString() != "{}"
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(t.Properties.RootElement.ToString())
                : new Dictionary<string, object>()
        }).ToList();

        var request = new ActionRequest(queue.Id, queue.MatchmakerFunction.ActionType,
            JsonSerializer.SerializeToDocument(new
            {
                tickets = ticketData,
                rules = new
                {
                    minPlayers = queue.MinPlayers,
                    maxPlayers = queue.MaxPlayers
                }
            }));

        var result = await actionService.ExecuteActionAsync(queue.Id, queue.GameId, request);
        if (!result.IsSuccess || result.Result.Data == null)
            return new List<List<MatchTicket>>();

        var groups = JsonSerializer.Deserialize<List<List<Guid>>>(result.Result.Data);
        return groups?.Select(group =>
                   tickets.Where(t => group.Contains(t.Id)).ToList()).ToList()
               ?? new List<List<MatchTicket>>();
    }

    private async Task<List<List<MatchTicket>>> ExecuteDefaultMatchmaker(List<MatchTicket> tickets,
        MatchmakingQueue queue)
    {
        var groups = new List<List<MatchTicket>>();
        var remainingTickets = tickets.ToList();

        while (remainingTickets.Count >= queue.MinPlayers)
        {
            var group = remainingTickets.Take(queue.MaxPlayers).ToList();
            groups.Add(group);
            remainingTickets = remainingTickets.Skip(queue.MaxPlayers).ToList();
        }

        return groups;
    }

    private MatchStatus MapMatchStatus(MatchStateStatus status)
    {
        return status switch
        {
            MatchStateStatus.Active => MatchStatus.InProgress,
            MatchStateStatus.Abandoned => MatchStatus.Abandoned,
            MatchStateStatus.Suspended => MatchStatus.Expired,
            MatchStateStatus.Completed => MatchStatus.Completed,
            _ => MatchStatus.Ready
        };
    }

    private async Task<MatchState> TransitionMatchState(Match match, MatchState currentState, string trigger,
        Guid playerId)
    {
        logger.LogDebug("Transitioning match {MatchId} state with trigger {Trigger}",
            match.Id, trigger);

        var queue = await queueRepository.GetByGameAndNameAsync(match.GameId, match.QueueName);

        var transitionResult = await actionService.ExecuteActionAsync(
            match.Id,
            match.GameId,
            new ActionRequest(match.Id, "match.transition",
                JsonSerializer.SerializeToDocument(new
                {
                    matchId = match.Id,
                    playerId = playerId.ToString(),
                    action = trigger,
                    state = currentState,
                    trigger,
                    rules = queue.Rules
                })
            )
        );

        return transitionResult.IsSuccess && transitionResult.Result.Data != null
            ? JsonSerializer.Deserialize<MatchState>(transitionResult.Result.Data)
            : currentState;
    }

    private PresenceState ValidateAndGetPresence(MatchState matchState, Guid playerId, Guid matchId)
    {
        var presence = matchState.Presences.FirstOrDefault(p => p.PlayerId == playerId.ToString());
        if (presence == null)
        {
            logger.LogWarning("Player {PlayerId} not found in match {MatchId}", playerId, matchId);
            throw new PlayerPresenceException(playerId, matchId, "Player not connected to match");
        }

        if (presence.Status != PresenceStatus.Connected)
        {
            logger.LogWarning("Player {PlayerId} presence status is {PresenceStatus}",
                playerId, presence.Status);
            throw new PlayerPresenceException(playerId, matchId,
                $"Presence status {presence.Status} invalid for this operation");
        }

        return presence;
    }

    public async Task<MatchActionResponse> SubmitActionAsync(Guid matchId, Guid playerId, MatchActionRequest action)
    {
        logger.LogInformation("Processing {ActionType} action for player {PlayerId} in match {MatchId}",
            action.ActionType, playerId, matchId);

        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.Deserialize<MatchState>();
        if (matchState == null)
            throw new InvalidOperationException("Invalid match state");

        _ = ValidateAndGetPresence(matchState, playerId, matchId);

        var queue = await queueRepository.GetByGameAndNameAsync(match.GameId, match.QueueName);

        var actionResult = await actionService.ExecuteActionAsync(
            match.Id,
            match.GameId,
            new ActionRequest(match.Id, action.ActionType, JsonSerializer.SerializeToDocument(new
                {
                    matchId,
                    playerId,
                    action = action.ActionData,
                    state = matchState,
                    rules = queue.Rules
                })
            )
        );

        if (!actionResult.IsSuccess)
            throw new ApplicationException(actionResult.ErrorMessage ?? "Invalid action");

        var matchAction = await CreateMatchAction(matchId, playerId, action);

        var newState = await TransitionMatchState(match, matchState, action.ActionType, playerId);
        match.MatchState = JsonSerializer.SerializeToDocument(newState);
        match.State = MapMatchStatus(newState.Status);
        match.LastActionAt = DateTime.UtcNow;

        if (match.State == MatchStatus.Completed)
        {
            match.CompletedAt = DateTime.UtcNow;
        }

        await matchRepository.UpdateAsync(match);

        logger.LogInformation("Action {ActionType} processed successfully for match {MatchId}",
            action.ActionType, matchId);

        return mapper.Map<MatchActionResponse>(matchAction);
    }

    private async Task<MatchAction> CreateMatchAction(Guid matchId, Guid playerId, MatchActionRequest action)
    {
        var matchAction = new MatchAction
        {
            MatchId = matchId,
            PlayerId = playerId,
            ActionType = action.ActionType,
            ActionData = action.ActionData,
            Timestamp = DateTime.UtcNow
        };

        await actionRepository.CreateAsync(matchAction);
        return matchAction;
    }

    public async Task<MatchResponse?> GetMatchAsync(Guid matchId) =>
        mapper.Map<MatchResponse>(await matchRepository.GetByIdAsync(matchId));

    public async Task<JsonDocument> GetMatchStateAsync(Guid matchId)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        return match?.MatchState ?? throw new NotFoundException("Match", matchId);
    }

    public async Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null,
        int? limit = null)
    {
        var actions = await actionRepository.GetMatchActionsAsync(matchId, since, limit);
        return actions.Select(a => mapper.Map<MatchActionResponse>(a)).ToList();
    }

    public async Task<MatchResponse> UpdatePresenceAsync(Guid matchId, Guid playerId, string sessionId,
        PresenceStatus status, JsonDocument meta)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        if (!match.PlayerIds.Contains(playerId))
            throw new InvalidOperationException("Player is not a participant in this match");

        var matchState = match.MatchState.Deserialize<MatchState>();
        var newState = await HandleMatchEvent(match, "presence_update", new
        {
            matchState,
            playerId = playerId.ToString(),
            sessionId,
            status = (int)status,
            meta
        });

        match.MatchState = JsonSerializer.SerializeToDocument(newState);
        match.State = MapMatchStatus(newState.Status);
        match.LastActionAt = DateTime.UtcNow;

        await matchRepository.UpdateAsync(match);
        return mapper.Map<MatchResponse>(match);
    }

    public async Task<MatchResponse> MarkPlayerReadyAsync(Guid matchId, Guid playerId)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.Deserialize<MatchState>();
        var presence = matchState?.Presences.FirstOrDefault(p => p.PlayerId == playerId.ToString());

        if (presence != null)
        {
            presence.Meta = JsonSerializer.SerializeToDocument(new
            {
                ready = true,
                readyAt = DateTime.UtcNow
            });
        }

        var allReady = matchState?.Presences.All(p =>
            p.Meta.RootElement.TryGetProperty("ready", out var ready) && ready.GetBoolean());

        if (allReady.GetValueOrDefault())
        {
            matchState.Status = MatchStateStatus.Active;
            matchState.Phase = MatchPhase.Playing;
            matchState.StartedAt = DateTime.UtcNow;
        }

        match.MatchState = JsonSerializer.SerializeToDocument(matchState);
        match.State = allReady.GetValueOrDefault() ? MatchStatus.InProgress : MatchStatus.Ready;

        await matchRepository.UpdateAsync(match);
        return mapper.Map<MatchResponse>(match);
    }

    private async Task<MatchState> HandleMatchEvent(Match match, string eventType, object eventData)
    {
        var matchState = match.MatchState.Deserialize<MatchState>();
        var queue = await queueRepository.GetByGameAndNameAsync(match.GameId, match.QueueName);

        var result = await actionService.ExecuteActionAsync(
            match.Id,
            match.GameId,
            new ActionRequest(match.Id, $"match.{eventType}",
                JsonSerializer.SerializeToDocument(new
                {
                    state = matchState,
                    presenceEvent = eventData,
                    rules = queue.Rules
                })
            )
        );

        return result.IsSuccess && result.Result.Data != null
            ? JsonSerializer.Deserialize<MatchState>(result.Result.Data)
            : matchState;
    }
}