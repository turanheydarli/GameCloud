using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class MatchmakingService : IMatchmakingService
{
    private readonly IMatchmakingQueueRepository _queueRepository;
    private readonly IMatchTicketRepository _ticketRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchActionRepository _actionRepository;
    private readonly IFunctionRepository _functionRepository;
    private readonly IMapper _mapper;
    private readonly IGameContext _gameContext;
    private readonly IActionService _actionService;

    public MatchmakingService(
        IMatchmakingQueueRepository queueRepository,
        IMatchTicketRepository ticketRepository,
        IMatchRepository matchRepository,
        IMatchActionRepository actionRepository,
        IMapper mapper,
        IGameContext gameContext,
        IActionService actionService, IFunctionRepository functionRepository)
    {
        _queueRepository = queueRepository;
        _ticketRepository = ticketRepository;
        _matchRepository = matchRepository;
        _actionRepository = actionRepository;
        _mapper = mapper;
        _gameContext = gameContext;
        _actionService = actionService;
        _functionRepository = functionRepository;
    }

    public async Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest request)
    {
        var matchmaker =
            await _functionRepository.GetByActionTypeAsync(_gameContext.GameId, request.matchmakerFunctionName);

        if (matchmaker == null)
        {
            throw new NotFoundException("The matchmaker was not found");
        }

        var queue = new MatchmakingQueue
        {
            GameId = _gameContext.GameId,
            Name = request.Name,
            Description = request.Description,
            IsEnabled = true,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            TicketTTL = request.TicketTTL,
            Rules = request.Rules,
            MatchmakerFunctionId = matchmaker.Id
        };

        await _queueRepository.CreateAsync(queue);
        return _mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null,
        string? queueName = null)
    {
        MatchmakingQueue? queue = null;

        if (queueId.HasValue)
        {
            queue = await _queueRepository.GetByIdAsync(queueId.Value);
        }
        else if (gameId.HasValue && !string.IsNullOrWhiteSpace(queueName))
        {
            queue = await _queueRepository.GetByGameAndNameAsync(gameId.Value, queueName);
        }

        return queue == null ? null : _mapper.Map<MatchmakingResponse>(queue);
    }

    public async Task<MatchTicketResponse> CreateTicketAsync(
        Guid gameId,
        Guid playerId,
        string queueName,
        JsonDocument? properties = null)
    {
        var queue = await _queueRepository.GetByGameAndNameAsync(gameId, queueName);
        if (queue == null)
            throw new NotFoundException("Queue", $"{gameId}/{queueName}");

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

        await _ticketRepository.CreateAsync(ticket);
        return _mapper.Map<MatchTicketResponse>(ticket);
    }

    // Add these to your match state
    public class MatchPresence
    {
        public Guid PlayerId { get; set; }
        public string SessionId { get; set; }
        public string Status { get; set; } // "connected", "disconnected", "spectating"
        public DateTime LastSeen { get; set; }
        public JsonDocument Meta { get; set; }
        public DateTime JoinedAt { get; set; }
        public string? Mode { get; set; }
    }

    public async Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null)
    {
        var createdMatches = new List<Match>();
        var queues = queueId.HasValue
            ? new[] { await _queueRepository.GetByIdAsync(queueId.Value) }.Where(q => q != null)
            : await _queueRepository.GetAllAsync();
    
        foreach (var queue in queues.Where(q => q.IsEnabled))
        {
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(queue.Id);
            if (activeTickets.Count < queue.MinPlayers) continue;
    
            var matchedGroups = await FindMatchingPlayers(activeTickets, queue);
            foreach (var group in matchedGroups)
            {
                var initialState = await _actionService.ExecuteActionAsync(
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
                    State = MatchState.Ready,
                    CreatedAt = DateTime.UtcNow,
                    LastActionAt = DateTime.UtcNow,
                    PlayerIds = group.Select(t => t.PlayerId).ToList(),
                    PlayerStates = JsonSerializer.SerializeToDocument(
                        group.Select(t => t.Properties ?? JsonDocument.Parse("{}"))
                    ),
                    MatchState = initialState.Result.Data ?? JsonSerializer.SerializeToDocument(new
                    {
                        status = "created", // Match lifecycle state
                        phase = "joining",  // Game-specific phase
                        startedAt = (DateTime?)null,
                        presences = group.Select(t => new MatchPresence
                        {
                            PlayerId = t.PlayerId,
                            SessionId = Guid.NewGuid().ToString(), // Should come from actual session
                            Status = "connected",
                            LastSeen = DateTime.UtcNow,
                            Meta = t.Properties ?? JsonDocument.Parse("{}")
                        }).ToList(),
                        state = queue.Rules.RootElement.TryGetProperty("initialState", out var initialStateValue)
                            ? JsonDocument.Parse(initialStateValue.ToString())
                            : JsonDocument.Parse("{}"),
                        metadata = new { }
                    }),
                    TurnHistory = JsonDocument.Parse("[]")
                };
    
                await _matchRepository.CreateAsync(match);
                createdMatches.Add(match);
    
                // Update ticket statuses to Matched
                foreach (var ticket in group)
                {
                    ticket.Status = TicketStatus.Matched;
                    ticket.MatchId = match.Id;

                    await _ticketRepository.UpdateAsync(ticket);
                }
            }
        }
    
        return createdMatches.Select(m => _mapper.Map<MatchResponse>(m)).ToList();
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

        var result = await _actionService.ExecuteActionAsync(queue.Id, queue.GameId, request);
        if (!result.IsSuccess || result.Result.Data == null)
            return new List<List<MatchTicket>>();

        var jsonString = result.Result.Data.RootElement.GetString();
        if (string.IsNullOrEmpty(jsonString))
            return new List<List<MatchTicket>>();

        var groups = JsonSerializer.Deserialize<List<List<Guid>>>(jsonString);
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

    public async Task<MatchActionResponse> SubmitActionAsync(Guid matchId, Guid playerId, MatchActionRequest action)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);

        var queue = await _queueRepository.GetByGameAndNameAsync(match.GameId, match.QueueName);
        JsonElement? actionHandler = null;
        if (queue.Rules != null && 
            queue.Rules.RootElement.TryGetProperty("actions", out JsonElement actions) && 
            actions.TryGetProperty(action.ActionType, out JsonElement handler))
        {
            actionHandler = handler;
        }
        
        var actionResult = await _actionService.ExecuteActionAsync(
            match.Id,
            match.GameId,
            new ActionRequest(match.Id, 
                actionHandler?.GetProperty("handler").GetString() ?? action.ActionType,
                JsonSerializer.SerializeToDocument(new
                {
                    matchId,
                    playerId,
                    action = action.ActionData,
                    state = match.MatchState,
                    players = match.PlayerStates,
                    rules = actionHandler.HasValue ? actionHandler.Value.GetProperty("rules") : JsonDocument.Parse("{}").RootElement
                })
            )
        );

        if (!actionResult.IsSuccess)
            throw new ApplicationException(actionResult.ErrorMessage ?? "Invalid action");

        var matchAction = new MatchAction
        {
            MatchId = matchId,
            PlayerId = playerId,
            ActionType = action.ActionType,
            ActionData = action.ActionData,
            Timestamp = DateTime.UtcNow
        };
        await _actionRepository.CreateAsync(matchAction);

        match.LastActionAt = DateTime.UtcNow;
        match.MatchState = actionResult.Result.Data ?? match.MatchState;
        await _matchRepository.UpdateAsync(match);

        return _mapper.Map<MatchActionResponse>(matchAction);
    }

    public async Task<MatchResponse?> GetMatchAsync(Guid matchId) =>
        _mapper.Map<MatchResponse>(await _matchRepository.GetByIdAsync(matchId));

    public async Task<JsonDocument> GetMatchStateAsync(Guid matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        return match?.MatchState ?? throw new NotFoundException("Match", matchId);
    }

    public async Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null,
        int? limit = null)
    {
        var actions = await _actionRepository.GetMatchActionsAsync(matchId, since, limit);
        return actions.Select(a => _mapper.Map<MatchActionResponse>(a)).ToList();
    }
    public async Task<MatchResponse> UpdatePresenceAsync(Guid matchId, Guid playerId, string sessionId, string status, JsonDocument meta)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);
    
        var state = match.MatchState.Deserialize<dynamic>();
        var presences = ((JsonElement)state.presences).Deserialize<List<MatchPresence>>();
        
        var presence = presences.FirstOrDefault(p => p.PlayerId == playerId);
        if (presence == null)
        {
            presence = new MatchPresence
            {
                PlayerId = playerId,
                SessionId = sessionId,
                Status = status,
                Mode = meta.RootElement.TryGetProperty("mode", out var mode) ? mode.GetString() : "player",
                JoinedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                Meta = meta,
            };
            presences.Add(presence);
        }
        else
        {
            presence.Status = status;
            presence.LastSeen = DateTime.UtcNow;
            presence.Meta = meta;
        }
    
        var activePlayers = presences.Count(p => p.Mode == "player" && p.Status == "connected");
        var requiredPlayers = state.metadata.GetProperty("minPlayers").GetInt32();
        
        state.status = DetermineMatchStatus(state.status.GetString(), activePlayers, requiredPlayers);
        state.phase = DetermineGamePhase(state.status.GetString(), state.phase.GetString());
        
        if (state.status.GetString() == "active" && state.startedAt == null)
        {
            state.startedAt = DateTime.UtcNow;
        }
    
        match.MatchState = JsonSerializer.SerializeToDocument(state);
        match.State = MapMatchStatus(state.status.GetString());
        
        await _matchRepository.UpdateAsync(match);
        return _mapper.Map<MatchResponse>(match);
    }

    private string DetermineMatchStatus(string currentStatus, int activePlayers, int requiredPlayers)
    {
        return currentStatus switch
        {
            "created" when activePlayers > 0 => "joining",
            "joining" when activePlayers >= requiredPlayers => "ready",
            "ready" when activePlayers < requiredPlayers => "joining",
            "active" when activePlayers < requiredPlayers => "suspended",
            "suspended" when activePlayers >= requiredPlayers => "active",
            _ when activePlayers == 0 => "abandoned",
            _ => currentStatus
        };
    }

    private string DetermineGamePhase(string matchStatus, string currentPhase)
    {
        return matchStatus switch
        {
            "created" => "initialization",
            "joining" => "waiting_players",
            "ready" => "ready_check",
            "active" when currentPhase == "ready_check" => "starting",
            "suspended" => "paused",
            "abandoned" => "cleanup",
            _ => currentPhase
        };
    }

    private MatchState MapMatchStatus(string status)
    {
        return status switch
        {
            "active" => MatchState.InProgress,
            "abandoned" => MatchState.Abandoned,
            "suspended" => MatchState.Expired,
            "completed" => MatchState.Completed,
            _ => MatchState.Ready
        };
    }

    public async Task<MatchResponse> MarkPlayerReadyAsync(Guid matchId, Guid playerId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new NotFoundException("Match", matchId);
    
        var state = JsonSerializer.Deserialize<dynamic>(match.MatchState);
        var presences = ((JsonElement)state.presences).Deserialize<List<MatchPresence>>();
        
        var presence = presences.FirstOrDefault(p => p.PlayerId == playerId);
        if (presence != null)
        {
            presence.Meta = JsonSerializer.SerializeToDocument(new
            {
                ready = true,
                readyAt = DateTime.UtcNow
            });
        }
    
        // Update match lifecycle
        var allReady = presences.All(p => 
            p.Meta.RootElement.TryGetProperty("ready", out var ready) && 
            ready.GetBoolean());
        
        if (allReady)
        {
            state.status = "active";
            state.phase = "playing";
            state.startedAt = DateTime.UtcNow;
        }
    
        match.MatchState = JsonSerializer.SerializeToDocument(state);
        match.State = state.status == "active" ? MatchState.InProgress : MatchState.Ready;
    
        await _matchRepository.UpdateAsync(match);
        return _mapper.Map<MatchResponse>(match);
    }
}