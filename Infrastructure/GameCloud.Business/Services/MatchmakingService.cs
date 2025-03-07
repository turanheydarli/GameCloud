using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
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
    IMatchStateCache matchStateCache,
    IStoredMatchRepository storedMatchRepository,
    IStoredPlayerRepository storedPlayerRepository,
    IPlayerService playerService,
    ILogger<MatchmakingService> logger)
    : IMatchmakingService
{
    public async Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest request)
    {
        logger.LogInformation("Creating match queue {QueueName} for game {GameId}", request.Name, gameContext.GameId);

        var queue = new MatchmakingQueue
        {
            GameId = gameContext.GameId,
            Name = request.Name,
            Description = request.Description,
            QueueType = request.QueueType,
            IsEnabled = true,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            TicketTTL = request.TicketTTL,
            Rules = request.Rules,
            UseCustomMatchmaker = request.UseCustomMatchmaker,
        };

        if (request.UseCustomMatchmaker)
        {
            var matchmaker = await functionRepository.GetAsync(m => m.GameId == gameContext.GameId
                                                                    && m.Id == request.MatchmakerFunctionId);

            if (matchmaker == null)
            {
                logger.LogWarning("Matchmaker function {FunctionName} not found for game {GameId}",
                    request.MatchmakerFunctionId, gameContext.GameId);
                throw new MatchmakerFunctionNotFoundException(request.MatchmakerFunctionId.ToString()!,
                    gameContext.GameId);
            }

            if (matchmaker == null)
            {
                throw new NotFoundException("The matchmaker was not found");
            }

            queue.MatchmakerFunctionId = matchmaker.Id;
            queue.MatchmakerFunction = matchmaker;
        }

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

            if (!activeTickets.Any()) continue;

            if (queue.QueueType == QueueType.Asynchronous)
            {
                foreach (var ticket in activeTickets)
                {
                    var matchedGroups = await FindMatchingPlayers(new List<MatchTicket> { ticket }, queue);
                    if (matchedGroups.Any())
                    {
                        createdMatches.AddRange(await CreateMatchesFromGroups(matchedGroups, queue));
                    }
                }

                continue;
            }

            if (activeTickets.Count < queue.MinPlayers) continue;

            var matchedRealGroups = await FindMatchingPlayers(activeTickets, queue);
            if (matchedRealGroups.Any())
            {
                createdMatches.AddRange(await CreateMatchesFromGroups(matchedRealGroups, queue));
            }
        }

        return createdMatches.Select(mapper.Map<MatchResponse>).ToList();
    }

    private async Task<List<Match>> CreateMatchesFromGroups(List<List<MatchTicket>> matchedGroups,
        MatchmakingQueue queue)
    {
        var createdMatches = new List<Match>();

        foreach (var group in matchedGroups)
        {
            var initialState = await actionService.ExecuteActionAsync(
                queue.Id,
                queue.GameId,
                new ActionRequest(queue.Id, "match.initialize",
                    JsonSerializer.SerializeToDocument(new
                    {
                        players = group.Select(t => JsonSerializer.Serialize(t.Player)),
                        rules = queue.Rules,
                        metadata = new
                        {
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
            matchState["presences"] = JsonSerializer.SerializeToElement(new List<Dictionary<string, object>>());
            match.MatchState = JsonSerializer.SerializeToDocument(matchState);

            await matchRepository.CreateAsync(match);
            createdMatches.Add(match);

            foreach (var ticket in group)
            {
                if (!ticket.IsStoredPlayer)
                {
                    ticket.Status = TicketStatus.Matched;
                    ticket.MatchId = match.Id;
                    await ticketRepository.UpdateAsync(ticket);
                }
            }
        }

        return createdMatches;
    }

    private async Task<List<List<MatchTicket>>> FindMatchingPlayers(List<MatchTicket> tickets, MatchmakingQueue queue)
    {
        if (queue.QueueType == QueueType.Asynchronous)
        {
            return await FindAsyncMatch(tickets.First(), queue);
        }

        if (queue is { UseCustomMatchmaker: true, MatchmakerFunctionId: not null })
        {
            return await ExecuteCustomMatchmaker(tickets, queue);
        }

        return await ExecuteDefaultMatchmaker(tickets, queue);
    }

    private async Task<StoredPlayerState> GetPlayerState(Guid playerId)
    {
        var recentMatches = await storedMatchRepository.GetByGameAndQueueAsync(
            gameContext.GameId,
            null,
            match => match.Players.Any(p => p.Id == playerId),
            10
        );

        return new StoredPlayerState
        {
            PlayerId = playerId,
            RecentOpponents = recentMatches?.SelectMany(m => m.Players)
                ?.Where(p => p.Id != playerId)
                ?.Select(p => p.Id)
                ?.ToList(),
            AverageScore = recentMatches?.SelectMany(m => m.Players)
                ?.Where(p => p.Id == playerId)
                ?.Select(p => p.Statistics?.Deserialize<PlayerMatchStatistics>()?.Score ?? 0)
                ?.DefaultIfEmpty(0)
                ?.Average() ?? 0,
            LastPlayedAt = recentMatches?.SelectMany(m => m.Players)
                ?.Where(p => p.Id == playerId)
                ?.Select(p => p.LastPlayedAt)
                ?.DefaultIfEmpty(DateTime.MinValue)
                ?.Max() ?? DateTime.MinValue
        };
    }

    private async Task<JsonDocument> PrepareReplayState(StoredMatch storedMatch, Guid activePlayerId)
    {
        var replayState = new
        {
            originalMatchId = storedMatch.OriginalMatchId,
            timestamp = DateTime.UtcNow,
            mode = "replay",
            activePlayer = activePlayerId,
            originalPlayers = storedMatch.Players.Select(p => new
            {
                PlayerId = p.Id,
                p.Statistics,
                p.LastPlayedAt
            }),
            gameState = storedMatch.GameState,
        };

        return JsonSerializer.SerializeToDocument(replayState);
    }

    private async Task<StoredMatch> CreateBotMatch(MatchTicket activeTicket, MatchmakingQueue queue)
    {
        var player = await playerService.GetByIdAsync(activeTicket.PlayerId);

        var botMatch = new StoredMatch
        {
            GameId = queue.GameId,
            QueueName = queue.Name,
            MatchType = "bot",
            Label = "bot-match",
            Metadata = JsonSerializer.SerializeToDocument(new
            {
                type = "bot_match",
                botConfig = new
                {
                    difficulty = "adaptive",
                    behavior = "default"
                }
            }),
            GameState = JsonSerializer.SerializeToDocument(new
            {
                type = "bot_match",
                virtualPlayers = new[]
                {
                    new
                    {
                        id = $"bot_{Guid.NewGuid()}",
                        type = "ai",
                        difficulty = "adaptive",
                        behavior = "default"
                    }
                },
                initialState = queue.Rules ?? JsonDocument.Parse("{}")
            }),
            Players = new List<StoredPlayer>(),
            IsAvailableForMatching = true,
            CompletedAt = DateTime.UtcNow,
        };

        botMatch = await storedMatchRepository.CreateAsync(botMatch);

        var matchHistoryEntry = await storedPlayerRepository.CreateAsync(new StoredPlayer
        {
            Id = activeTicket.PlayerId,
            LastPlayedAt = DateTime.UtcNow,
            Statistics = JsonSerializer.SerializeToDocument(new PlayerMatchStatistics
            {
                Score = 1000,
                IsBot = false,
            }),
            StoredMatchId = botMatch.Id,
            Actions = JsonDocument.Parse("{}"),
            Mode = "player",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PlayerId = player.Id
        });

        botMatch.Players.Add(matchHistoryEntry);
        await storedMatchRepository.UpdateAsync(botMatch);

        return botMatch;
    }

    private async Task<List<List<MatchTicket>>> FindAsyncMatch(MatchTicket activeTicket, MatchmakingQueue queue)
    {
        var playerState = await GetPlayerState(activeTicket.PlayerId);
        var playerProps = activeTicket.Properties.Deserialize<PlayerMatchProperties>();

        var storedMatches = await storedMatchRepository.GetByGameAndQueueAsync(
            queue.GameId,
            queue.Name,
            filter: match =>
                match.IsAvailableForMatching &&
                match.CompletedAt > DateTime.UtcNow.AddDays(-7) &&
                match.Players.Count <= queue.MaxPlayers &&
                !match.Players.Any(p => playerState.RecentOpponents.Contains(p.Id)),
            limit: queue.MaxPlayers - 1
        );

        if (!storedMatches.Any())
        {
            var botMatch = await CreateBotMatch(activeTicket, queue);
            storedMatches = new List<StoredMatch> { botMatch };
        }

        var matchGroup = new List<MatchTicket> { activeTicket };

        foreach (var storedMatch in storedMatches.OrderByDescending(m => CalculateMatchQuality(m, playerProps)))
        {
            foreach (var storedPlayer in storedMatch.Players.OrderBy(p => p.LastPlayedAt))
            {
                if (await IsPlayerInActiveMatch(storedPlayer.Id))
                    continue;

                var replayState = await PrepareReplayState(storedMatch, activeTicket.PlayerId);

                var virtualTicket = new MatchTicket
                {
                    Id = Guid.NewGuid(),
                    GameId = queue.GameId,
                    PlayerId = storedPlayer.Id,
                    QueueName = queue.Name,
                    Status = TicketStatus.Matched,
                    Properties = JsonSerializer.SerializeToDocument(new PlayerMatchProperties
                    {
                        StoredMatchId = storedMatch.Id,
                        Statistics = storedPlayer.Statistics != null
                            ? storedPlayer.Statistics.Deserialize<PlayerMatchStatistics>()
                            : new PlayerMatchStatistics(),
                        LastPlayedAt = storedPlayer.LastPlayedAt,
                        Mode = "async",
                        GameData = replayState
                    }),
                    IsStoredPlayer = true,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(queue.TicketTTL)
                };

                matchGroup.Add(virtualTicket);

                if (matchGroup.Count >= queue.MaxPlayers)
                    break;
            }

            if (matchGroup.Count >= queue.MinPlayers)
                break;
        }

        return matchGroup.Count >= queue.MinPlayers
            ? new List<List<MatchTicket>> { matchGroup }
            : new List<List<MatchTicket>>();
    }

    private double CalculateMatchQuality(StoredMatch match, PlayerMatchProperties activePlayerProps)
    {
        double score = 0;

        score += (DateTime.UtcNow - match.CompletedAt).TotalDays * -0.1;

        if (activePlayerProps.Statistics?.Score != null)
        {
            var avgMatchScore =
                match.Players.Average(p => p.Statistics.Deserialize<PlayerMatchStatistics>()?.Score ?? 0);
            var scoreDiff = Math.Abs(activePlayerProps.Statistics.Score - avgMatchScore);
            score += 1.0 / (1 + scoreDiff * 0.1);
        }

        return score;
    }

    public class StoredPlayerState
    {
        public Guid PlayerId { get; set; }
        public List<Guid>? RecentOpponents { get; set; } = new();
        public double AverageScore { get; set; }
        public DateTime LastPlayedAt { get; set; }
    }

    private async Task<bool> IsPlayerInActiveMatch(Guid playerId)
    {
        var activeMatch = await matchRepository.GetPlayerActiveMatchAsync(playerId);
        return activeMatch != null;
    }

    private async Task StoreCompletedMatch(Match match)
    {
        if (match.State != MatchStatus.Completed)
            return;

        var matchState = match.MatchState.Deserialize<MatchState>();

        var storedMatch = new StoredMatch
        {
            OriginalMatchId = match.Id,
            GameId = match.GameId,
            QueueName = match.QueueName,
            MatchType = matchState.MatchType,
            FinalScore = matchState.FinalScore,
            Duration = (match.CompletedAt - match.StartedAt)?.TotalSeconds ?? 0,
            CompletedAt = match.CompletedAt ?? DateTime.UtcNow,
            IsAvailableForMatching = true,
            Players = match.PlayerIds.Select(playerId => new StoredPlayer
            {
                Id = playerId,
                LastPlayedAt = match.CompletedAt ?? DateTime.UtcNow
            }).ToList()
        };

        await storedMatchRepository.CreateAsync(storedMatch);
        logger.LogInformation("Stored match metadata for {MatchId}", match.Id);
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

        var groups = result.Result.Data.Deserialize<List<List<Guid>>>();
        return groups?.Select(group => tickets.Where(t => group.Contains(t.Id)).ToList()).ToList()
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

        return await Task.FromResult(groups);
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

    private async Task<MatchState> TransitionMatchState(Match match, MatchState currentState,
        MatchActionRequest trigger,
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
                    action = trigger.ActionData,
                    state = currentState,
                    trigger = trigger.ActionType,
                    rules = queue.Rules
                })
            )
        );

        var newState = transitionResult.IsSuccess && transitionResult.Result.Data != null
            ? JsonSerializer.Deserialize<MatchState>(transitionResult.Result.Data)
            : currentState;

        await matchStateCache.SetMatchStateAsync(match.Id, newState);
        logger.LogDebug("Updated match state in cache after transition for {MatchId}", match.Id);

        return newState;
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

        var newState = await TransitionMatchState(match, matchState, action, playerId);
        match.MatchState = JsonSerializer.SerializeToDocument(newState);
        match.State = MapMatchStatus(newState.Status);
        match.LastActionAt = DateTime.UtcNow;

        // Update cache before database
        await matchStateCache.SetMatchStateAsync(matchId, newState);
        logger.LogDebug("Updated match state in cache after action for {MatchId}", matchId);

        if (match.State == MatchStatus.Completed)
        {
            match.CompletedAt = DateTime.UtcNow;
            await matchStateCache.RemoveMatchStateAsync(matchId);
            await StoreCompletedMatch(match);
            logger.LogInformation("Stored completed match {MatchId} for future matchmaking", matchId);
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
        var cachedState = await matchStateCache.GetMatchStateAsync(matchId);
        if (cachedState != null)
        {
            return JsonSerializer.SerializeToDocument(cachedState);
        }

        var match = await matchRepository.GetByIdAsync(matchId);
        if (match?.MatchState == null)
            throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.Deserialize<MatchState>();
        if (matchState != null)
        {
            await matchStateCache.SetMatchStateAsync(matchId, matchState);
            logger.LogDebug("Cached match state for {MatchId}", matchId);
        }

        return match.MatchState;
    }

    public async Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null,
        int? limit = null)
    {
        var actions = await actionRepository.GetMatchActionsAsync(matchId, since, limit);
        return actions.Select(a => mapper.Map<MatchActionResponse>(a)).ToList();
    }

    public async Task<MatchTicketResponse?> GetTicket(Guid gameId, Guid playerId, Guid ticketId)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId);

        return mapper.Map<MatchTicketResponse>(ticket);
    }

    public async Task<MatchResponse> UpdatePresenceAsync(Guid matchId, Guid playerId, string sessionId,
        PresenceStatus status, JsonDocument meta)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null) throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.RootElement.Deserialize<MatchState>
            (new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var presence = matchState.Presences.FirstOrDefault(p => p.PlayerId == playerId.ToString());
        if (presence == null)
        {
            matchState.Presences.Add(new PresenceState
            {
                PlayerId = playerId.ToString(),
                SessionId = sessionId,
                Status = status,
                Meta = meta,
                JoinedAt = DateTime.UtcNow
            });
        }
        else
        {
            presence.SessionId = sessionId;
            presence.Status = status;

            var existingMeta = presence.Meta?.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value) ?? new Dictionary<string, JsonElement>();
            var newMeta = meta.RootElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value);

            foreach (var (key, value) in newMeta)
            {
                existingMeta[key] = value;
            }

            presence.Meta = JsonSerializer.SerializeToDocument(existingMeta);
        }

        match.MatchState = JsonSerializer.SerializeToDocument(matchState);
        match.LastActionAt = DateTime.UtcNow;

        await matchStateCache.SetMatchStateAsync(matchId, matchState);
        logger.LogDebug("Updated presence in cache for match {MatchId}, player {PlayerId}", matchId, playerId);

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

        await matchStateCache.SetMatchStateAsync(matchId, matchState);
        logger.LogDebug("Updated ready state in cache for match {MatchId}, player {PlayerId}", matchId, playerId);

        if (allReady.GetValueOrDefault())
        {
            logger.LogInformation("All players ready in match {MatchId}, transitioning to active state", matchId);
        }

        await matchRepository.UpdateAsync(match);
        return mapper.Map<MatchResponse>(match);
    }

    private async Task<MatchState> HandleMatchEvent(Match match, string eventType, object eventData)
    {
        var matchState = match.MatchState.Deserialize<MatchState>();
        var queue = await queueRepository.GetByGameAndNameAsync(match.GameId, match.QueueName);
        ActionResponse result = null;
        try
        {
            result = await actionService.ExecuteActionAsync(
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
        }
        catch (NotFoundException exception)
        {
            logger.LogWarning("Action not found for event {EventType} in match {MatchId}: {ErrorMessage}",
                eventType, match.Id, exception.Message);
        }

        var newState = result is { IsSuccess: true, Result.Data: not null }
            ? result.Result.Data.Deserialize<MatchState>()
            : matchState;

        await matchStateCache.SetMatchStateAsync(match.Id, newState);
        logger.LogDebug("Updated match state in cache after {EventType} event", eventType);

        return newState;
    }

    public async Task<MatchResponse> EndMatchAsync(Guid matchId, Guid playerId, JsonDocument? finalState = null)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.Deserialize<MatchState>();
        var presence = ValidateAndGetPresence(matchState, playerId, matchId);

        // Handle match termination through match event system
        var newState = await HandleMatchEvent(match, "end", new
        {
            playerId = playerId.ToString(),
            finalState = finalState,
            reason = "completed"
        });

        match.State = MapMatchStatus(newState.Status);
        match.CompletedAt = DateTime.UtcNow;
        match.MatchState = JsonSerializer.SerializeToDocument(newState);

        await matchStateCache.RemoveMatchStateAsync(matchId);
        await StoreCompletedMatch(match);
        await matchRepository.UpdateAsync(match);

        return mapper.Map<MatchResponse>(match);
    }

    public async Task<MatchResponse> LeaveMatchAsync(Guid matchId, Guid playerId)
    {
        var match = await matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        var matchState = match.MatchState.Deserialize<MatchState>();
        var presence = matchState.Presences.FirstOrDefault(p => p.PlayerId == playerId.ToString());

        if (presence != null)
        {
            var newState = await HandleMatchEvent(match, "leave", new
            {
                playerId = playerId.ToString(),
                reason = "player_left"
            });

            match.MatchState = JsonSerializer.SerializeToDocument(newState);
            match.LastActionAt = DateTime.UtcNow;

            var remainingPlayers = newState.Presences.Count(p => p.Status == PresenceStatus.Connected);

            // TODO: While database normalization rules not match here, hardcoded minimum players
            if (remainingPlayers < 2) // HACK: Hardcoded minimum players
            {
                newState.Status = MatchStateStatus.Abandoned;
                match.State = MatchStatus.Abandoned;
                match.CompletedAt = DateTime.UtcNow;
                await matchStateCache.RemoveMatchStateAsync(matchId);
                await StoreCompletedMatch(match);
            }

            await matchStateCache.SetMatchStateAsync(matchId, newState);
            await matchRepository.UpdateAsync(match);
        }

        return mapper.Map<MatchResponse>(match);
    }

    public async Task<QueueActivityResponse> GetQueueActivityAsync(Guid queueId, string timeRange)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        var (startDate, endDate) = ParseTimeRange(timeRange);
        var activity = await matchRepository.GetQueueActivityAsync(queueId, startDate, endDate);

        return new QueueActivityResponse
        {
            Labels = activity.TimePoints,
            Matches = activity.MatchCounts,
            Players = activity.PlayerCounts
        };
    }

    public async Task<PageableListResponse<MatchResponse>> GetQueueMatchesAsync(Guid queueId, string? status,
        PageableRequest request)
    {
        var matches = await matchRepository.GetQueueMatchesAsync(
            queueId,
            status,
            request.PageIndex,
            request.PageSize
        );

        return mapper.Map<PageableListResponse<MatchResponse>>(matches);
    }

    public async Task<PageableListResponse<MatchTicketResponse>> GetQueueTicketsAsync(Guid queueId,
        PageableRequest request)
    {
        var tickets = await ticketRepository.GetQueueTicketsAsync(
            queueId,
            request.PageIndex,
            request.PageSize
        );

        return mapper.Map<PageableListResponse<MatchTicketResponse>>(tickets);
    }

    private async Task<QueueStatsResponse> CalculateQueueStats(Guid queueId, string timeRange)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        var (startDate, endDate) = ParseTimeRange(timeRange);

        var activeMatches = await matchRepository.GetQueueMatchesAsync(
            queueId,
            "InProgress",
            0,
            1000
        );

        // Get waiting tickets
        var waitingTickets = await ticketRepository.GetActiveTicketsAsync(queueId);

        // Get historical matches for calculating averages
        var historicalMatches = await matchRepository.GetQueueMatchesAsync(
            queueId,
            null,
            0,
            1000
        );

        var completedMatches = historicalMatches.Items
            .Where(m => m.State == MatchStatus.Completed)
            .ToList();

        // Calculate average wait time
        var avgWaitTime = waitingTickets.Any()
            ? waitingTickets.Average(t => (DateTime.UtcNow - t.CreatedAt).TotalSeconds)
            : 0;

        // Calculate previous period stats for trend
        var previousStartDate = startDate.AddDays(-(endDate - startDate).Days);
        var previousMatches = await matchRepository.GetQueueMatchesAsync(
            queueId,
            null,
            0,
            1000
        );

        var previousAvgWaitTime = previousMatches.Items
            .Where(m => m.CreatedAt >= previousStartDate && m.CreatedAt < startDate)
            .Average(m => ((m.StartedAt ?? m.CreatedAt) - m.CreatedAt).TotalSeconds);

        var waitTimeChange = previousAvgWaitTime > 0
            ? ((avgWaitTime - previousAvgWaitTime) / previousAvgWaitTime) * 100
            : 0;

        var waitTimeTrend = waitTimeChange switch
        {
            > 0 => "up",
            < 0 => "down",
            _ => "stable"
        };

        var totalMatchAttempts = completedMatches.Count + waitingTickets.Count;
        var successRate = totalMatchAttempts > 0
            ? (double)completedMatches.Count / totalMatchAttempts * 100
            : 0;

        return new QueueStatsResponse
        {
            QueueId = queueId,
            ActiveMatches = activeMatches.Count,
            WaitingPlayers = waitingTickets.Count,
            AvgWaitTimeSeconds = avgWaitTime,
            TotalPlayers = historicalMatches.Items.Sum(m => m.PlayerIds.Count),
            AverageWaitTime = avgWaitTime,
            AverageWaitTimeTrend = waitTimeTrend,
            AverageWaitTimeChange = Math.Abs(waitTimeChange),
            MatchmakingSuccessRate = successRate
        };
    }

    public async Task<QueueDashboardResponse> GetQueueDashboardAsync(Guid queueId)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        var stats = await CalculateQueueStats(queueId, "24h");
        var functions = await GetQueueFunctionsAsync(queueId);

        return new QueueDashboardResponse
        {
            Queue = mapper.Map<MatchmakingResponse>(queue),
            Stats = stats,
            Functions = functions
        };
    }

    public async Task<QueueFunctionsResponse> GetQueueFunctionsAsync(Guid queueId)
    {
        var queue = await queueRepository.GetByIdWithFunctionsAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        return new QueueFunctionsResponse
        {
            Initialize = queue.InitializeFunction != null
                ? new QueueFunctionInfo(
                    queue.InitializeFunction.Id,
                    queue.InitializeFunction.Name,
                    queue.InitializeFunction.ActionType)
                : null,
            Transition = queue.TransitionFunction != null
                ? new QueueFunctionInfo(
                    queue.TransitionFunction.Id,
                    queue.TransitionFunction.Name,
                    queue.TransitionFunction.ActionType)
                : null,
            Leave = queue.LeaveFunction != null
                ? new QueueFunctionInfo(
                    queue.LeaveFunction.Id,
                    queue.LeaveFunction.Name,
                    queue.LeaveFunction.ActionType)
                : null,
            End = queue.EndFunction != null
                ? new QueueFunctionInfo(
                    queue.EndFunction.Id,
                    queue.EndFunction.Name,
                    queue.EndFunction.ActionType)
                : null
        };
    }

    public async Task UpdateQueueFunctionsAsync(Guid queueId, UpdateQueueFunctionsRequest request)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        queue.InitializeFunctionId = request.InitializeFunctionId;
        queue.TransitionFunctionId = request.TransitionFunctionId;
        queue.LeaveFunctionId = request.LeaveFunctionId;
        queue.EndFunctionId = request.EndFunctionId;

        await queueRepository.UpdateAsync(queue);
    }

    public async Task<JsonDocument> GetQueueRulesAsync(Guid queueId)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        return queue.Rules;
    }

    public async Task UpdateQueueRulesAsync(Guid queueId, JsonDocument rules)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        queue.Rules = rules;
        await queueRepository.UpdateAsync(queue);
    }

    public async Task<QueueTestResponse> TestQueueAsync(Guid queueId, QueueTestRequest request)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        var startTime = DateTime.UtcNow;
        var testPlayers = new List<TestPlayer>();

        for (var i = 0; i < request.Players; i++)
        {
            testPlayers.Add(new TestPlayer(
                Guid.NewGuid(),
                request.Properties
            ));
        }

        var tickets = testPlayers.Select(p => new MatchTicket
        {
            GameId = queue.GameId,
            PlayerId = p.Id,
            QueueName = queue.Name,
            Properties = p.Properties,
            Status = TicketStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(queue.TicketTTL)
        }).ToList();

        var matchedGroups = await FindMatchingPlayers(tickets, queue);
        if (!matchedGroups.Any())
            return new QueueTestResponse(false, Guid.Empty, testPlayers, 0);

        var matches = await CreateMatchesFromGroups(matchedGroups, queue);
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

        return new QueueTestResponse(
            true,
            matches.First().Id,
            testPlayers,
            duration
        );
    }

    private (DateTime startDate, DateTime endDate) ParseTimeRange(string timeRange)
    {
        var end = DateTime.UtcNow;
        var start = timeRange switch
        {
            "24h" => end.AddHours(-24),
            "7d" => end.AddDays(-7),
            "30d" => end.AddDays(-30),
            _ => end.AddHours(-24)
        };
        return (start, end);
    }

    public async Task<PageableListResponse<MatchmakingResponse>> GetQueuesAsync(Guid gameId, string? search,
        PageableRequest request)
    {
        var queues = await queueRepository.GetPagedAsync(gameId, search, request.PageIndex, request.PageSize);
        return mapper.Map<PageableListResponse<MatchmakingResponse>>(queues);
    }

    public async Task<MatchmakingResponse> GetQueueDetailsAsync(Guid queueId)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        return mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest request)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        queue.Name = request.Name;
        queue.Description = request.Description;
        queue.QueueType = request.QueueType;
        queue.MinPlayers = request.MinPlayers;
        queue.MaxPlayers = request.MaxPlayers;
        queue.TicketTTL = request.TicketTTL;
        queue.Rules = request.Rules;
        queue.UseCustomMatchmaker = request.UseCustomMatchmaker;

        if (request.UseCustomMatchmaker)
        {
            var matchmaker = await functionRepository.GetAsync(m =>
                m.GameId == queue.GameId &&
                m.Id == request.MatchmakerFunctionId
            );

            if (matchmaker == null)
                throw new MatchmakerFunctionNotFoundException(request.MatchmakerFunctionId.ToString()!,
                    gameContext.GameId);

            queue.MatchmakerFunctionId = matchmaker.Id;
            queue.MatchmakerFunction = matchmaker;
        }
        else
        {
            queue.MatchmakerFunctionId = null;
            queue.MatchmakerFunction = null;
        }

        await queueRepository.UpdateAsync(queue);
        return mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task DeleteQueueAsync(Guid queueId)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        await queueRepository.DeleteAsync(queue);
    }

    public async Task<QueueToggleResponse> ToggleQueueAsync(Guid queueId, bool isEnabled)
    {
        var queue = await queueRepository.GetByIdAsync(queueId);
        if (queue == null)
            throw new NotFoundException("Queue", queueId);

        queue.IsEnabled = isEnabled;
        await queueRepository.UpdateAsync(queue);

        return new QueueToggleResponse(true, IsEnabled: queue.IsEnabled, QueueId: queue.Id);
    }

    public async Task<MatchmakingStatsResponse> GetMatchmakingStatsAsync(Guid gameId, List<Guid>? queueIds,
        string timeRange)
    {
        var stats = new List<QueueStatsResponse>();
        var queues = queueIds != null && queueIds.Any()
            ? await queueRepository.GetByIdsAsync(queueIds)
            : await queueRepository.GetByGameIdAsync(gameId);

        foreach (var queue in queues)
        {
            var queueStats = await CalculateQueueStats(queue.Id, timeRange);
            stats.Add(queueStats);
        }

        return new MatchmakingStatsResponse
        {
            QueueStats = stats
        };
    }
}