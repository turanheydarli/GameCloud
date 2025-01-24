using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class MatchmakingService : IMatchmakingService
{
    // private readonly IMatchmakingQueueRepository _queueRepository;
    // private readonly IMatchTicketRepository _ticketRepository;
    // private readonly IMatchRepository _matchRepository;
    // private readonly IMapper _mapper;
    // private readonly IGameContext _gameContext;
    // private readonly IPlayerAttributeRepository _playerAttributeRepository;
    //
    // public MatchmakingService(
    //     IMatchmakingQueueRepository queueRepository,
    //     IMatchTicketRepository ticketRepository,
    //     IMatchRepository matchRepository,
    //     IMapper mapper,
    //     IGameContext gameContext,
    //     IPlayerAttributeRepository playerAttributeRepository)
    // {
    //     _queueRepository = queueRepository;
    //     _ticketRepository = ticketRepository;
    //     _matchRepository = matchRepository;
    //     _mapper = mapper;
    //     _gameContext = gameContext;
    //     _playerAttributeRepository = playerAttributeRepository;
    // }
    //
    // #region Offline / Single-Player
    //
    // public async Task<MatchResponse> CreateOfflineMatchAsync(OfflineMatchRequest request, Guid playerId)
    // {
    //     var attacker = playerId;
    //     var offlineOpponentId = await FindOfflineOpponentAsync(request.Criteria);
    //     if (offlineOpponentId == null) throw new ApplicationException("No valid offline opponent found.");
    //
    //     var match = new Match
    //     {
    //         GameId = _gameContext.GameId,
    //         QueueName = request.QueueName ?? "OfflineAttackQueue",
    //         State = MatchState.Created,
    //         PlayerIds = new List<Guid> { attacker, offlineOpponentId.Value },
    //         CreatedAt = DateTime.UtcNow,
    //         MatchData = JsonDocument.Parse("{}")
    //     };
    //     await _matchRepository.CreateAsync(match);
    //     return _mapper.Map<MatchResponse>(match);
    // }
    //
    // public async Task<Guid?> FindOfflineOpponentAsync(MatchingCriteria? criteria)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // #endregion
    //
    // #region Queue Management
    //
    // public async Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest)
    // {
    //     var existing = await _queueRepository.GetByGameAndNameAsync(queueRequest.GameId, queueRequest.Name);
    //     if (existing != null) throw new ApplicationException($"Queue '{queueRequest.Name}' exists.");
    //
    //     var queue = new MatchmakingQueue
    //     {
    //         GameId = _gameContext.GameId,
    //         Name = queueRequest.Name,
    //         MinPlayers = queueRequest.MinPlayers,
    //         MaxPlayers = queueRequest.MaxPlayers,
    //         TicketTTL = queueRequest.TicketTTL,
    //         Criteria = JsonSerializer.SerializeToDocument(queueRequest.Criteria),
    //         CreatedAt = DateTime.UtcNow,
    //         UpdatedAt = DateTime.UtcNow
    //     };
    //     await _queueRepository.CreateAsync(queue);
    //     return _mapper.Map<MatchmakingResponse>(queue);
    // }
    //
    // public async Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest queueRequest)
    // {
    //     var queue = await _queueRepository.GetByIdAsync(queueId);
    //     if (queue == null) throw new NotFoundException("MatchmakingQueue", queueId);
    //
    //     queue.Name = queueRequest.Name;
    //     queue.MinPlayers = queueRequest.MinPlayers;
    //     queue.MaxPlayers = queueRequest.MaxPlayers;
    //     queue.TicketTTL = queueRequest.TicketTTL;
    //     queue.UpdatedAt = DateTime.UtcNow;
    //     await _queueRepository.UpdateAsync(queue);
    //
    //     return _mapper.Map<MatchmakingResponse>(queue);
    // }
    //
    // public async Task DeleteQueueAsync(Guid queueId)
    // {
    //     var queue = await _queueRepository.GetByIdAsync(queueId);
    //     if (queue != null) await _queueRepository.DeleteAsync(queue);
    // }
    //
    // public async Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null,
    //     string? queueName = null)
    // {
    //     MatchmakingQueue? queue = null;
    //     if (queueId.HasValue) queue = await _queueRepository.GetByIdAsync(queueId.Value);
    //     else if (gameId.HasValue && !string.IsNullOrWhiteSpace(queueName))
    //         queue = await _queueRepository.GetByGameAndNameAsync(gameId.Value, queueName);
    //
    //     return queue == null ? null : _mapper.Map<MatchmakingResponse>(queue);
    // }
    //
    // #endregion
    //
    // #region Standard Find/Enqueue
    //
    // public async Task<MatchResponse> FindOrCreateMatchAsync(FindMatchRequest request, Guid playerId)
    // {
    //     var ticket =
    //         await EnqueuePlayerAsync(_gameContext.GameId, playerId, request.QueueName, request.CustomProperties);
    //     await ProcessMatchmakingAsync();
    //     var updatedTicket = await _ticketRepository.GetByIdAsync(ticket.Id);
    //     if (updatedTicket?.MatchId != null)
    //     {
    //         var match = await _matchRepository.GetByIdAsync(updatedTicket.MatchId.Value);
    //         if (match != null) return _mapper.Map<MatchResponse>(match);
    //     }
    //
    //     throw new ApplicationException("No match formed yet. Try again or poll status.");
    // }
    //
    // public async Task<MatchTicketResponse> EnqueuePlayerAsync(Guid gameId, Guid playerId, string queueName,
    //     JsonDocument? customProperties = null)
    // {
    //     var queue = await _queueRepository.GetByGameAndNameAsync(gameId, queueName);
    //     if (queue == null) throw new NotFoundException("MatchmakingQueue", $"{gameId}/{queueName}");
    //
    //     var ticket = new MatchTicket
    //     {
    //         GameId = gameId,
    //         PlayerId = playerId,
    //         QueueName = queue.Name,
    //         Status = TicketStatus.Queued,
    //         CustomProperties = customProperties,
    //         CreatedAt = DateTime.UtcNow,
    //         ExpiresAt = DateTime.UtcNow.Add(queue.TicketTTL)
    //     };
    //     await _ticketRepository.CreateAsync(ticket);
    //     return _mapper.Map<MatchTicketResponse>(ticket);
    // }
    //
    // #endregion
    //
    // #region Ticket Lifecycle
    //
    // public async Task CancelTicketAsync(Guid ticketId)
    // {
    //     var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    //     if (ticket != null && (ticket.Status == TicketStatus.Queued || ticket.Status == TicketStatus.Matching))
    //     {
    //         ticket.Status = TicketStatus.Cancelled;
    //         ticket.UpdatedAt = DateTime.UtcNow;
    //         await _ticketRepository.UpdateAsync(ticket);
    //     }
    // }
    //
    // public async Task<MatchTicketResponse?> GetTicketAsync(Guid ticketId)
    // {
    //     var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    //     return ticket == null ? null : _mapper.Map<MatchTicketResponse>(ticket);
    // }
    //
    // public async Task AcceptMatchAsync(Guid ticketId)
    // {
    //     var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    //     if (ticket == null) throw new NotFoundException("MatchTicket", ticketId);
    //     if (ticket.Status == TicketStatus.MatchFound)
    //     {
    //         ticket.Status = TicketStatus.Accepted;
    //         ticket.UpdatedAt = DateTime.UtcNow;
    //         await _ticketRepository.UpdateAsync(ticket);
    //     }
    // }
    //
    // public async Task DeclineMatchAsync(Guid ticketId)
    // {
    //     var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    //     if (ticket == null) throw new NotFoundException("MatchTicket", ticketId);
    //     if (ticket.Status == TicketStatus.MatchFound)
    //     {
    //         ticket.Status = TicketStatus.Declined;
    //         ticket.UpdatedAt = DateTime.UtcNow;
    //         await _ticketRepository.UpdateAsync(ticket);
    //     }
    // }
    //
    // #endregion
    //
    // #region Match Lifecycle
    //
    // public async Task<MatchResponse?> GetMatchAsync(Guid matchId)
    // {
    //     var match = await _matchRepository.GetByIdAsync(matchId);
    //     return match == null ? null : _mapper.Map<MatchResponse>(match);
    // }
    //
    // public async Task<MatchResponse> UpdateMatchStateAsync(Guid matchId, MatchState newState)
    // {
    //     var match = await _matchRepository.GetByIdAsync(matchId);
    //     if (match == null) throw new NotFoundException("Match", matchId);
    //
    //     match.State = newState;
    //     match.UpdatedAt = DateTime.UtcNow;
    //     await _matchRepository.UpdateAsync(match);
    //     return _mapper.Map<MatchResponse>(match);
    // }
    //
    // public async Task CancelMatchAsync(Guid matchId)
    // {
    //     var match = await _matchRepository.GetByIdAsync(matchId);
    //     if (match != null)
    //     {
    //         match.State = MatchState.Cancelled;
    //         match.UpdatedAt = DateTime.UtcNow;
    //         await _matchRepository.UpdateAsync(match);
    //     }
    // }
    //
    // #endregion
    //
    // #region Process Matchmaking (Multi-player)
    //
    // public async Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null)
    // {
    //     var createdMatches = new List<Match>();
    //     var queues = new List<MatchmakingQueue>();
    //     if (queueId.HasValue)
    //     {
    //         var q = await _queueRepository.GetByIdAsync(queueId.Value);
    //         if (q != null) queues.Add(q);
    //     }
    //     else
    //     {
    //         queues.AddRange(await _queueRepository.GetAllAsync());
    //     }
    //
    //     foreach (var q in queues)
    //     {
    //         while (true)
    //         {
    //             var tickets = await _ticketRepository.GetActiveTicketsAsync(q.Id);
    //             if (tickets.Count < q.MinPlayers) break;
    //
    //             var group = await MatchPlayers(tickets, q);
    //             if (!group.Any()) break;
    //
    //             foreach (var t in group)
    //             {
    //                 t.Status = TicketStatus.Matching;
    //                 t.UpdatedAt = DateTime.UtcNow;
    //             }
    //
    //             await _ticketRepository.UpdateRangeAsync(group);
    //
    //             var match = new Match
    //             {
    //                 GameId = q.GameId,
    //                 QueueName = q.Name,
    //                 State = MatchState.Created,
    //                 PlayerIds = group.Select(t => t.PlayerId).ToList(),
    //                 CreatedAt = DateTime.UtcNow,
    //                 MatchData = JsonDocument.Parse("{}")
    //             };
    //             await _matchRepository.CreateAsync(match);
    //
    //             foreach (var t in group)
    //             {
    //                 t.MatchId = match.Id;
    //                 t.Status = TicketStatus.MatchFound;
    //                 t.UpdatedAt = DateTime.UtcNow;
    //             }
    //
    //             await _ticketRepository.UpdateRangeAsync(group);
    //
    //             createdMatches.Add(match);
    //         }
    //     }
    //
    //     return createdMatches.Select(m => _mapper.Map<MatchResponse>(m)).ToList();
    // }
    //
    // private async Task<List<MatchTicket>> MatchPlayers(List<MatchTicket> tickets, MatchmakingQueue q)
    // {
    //     if (tickets.Count < q.MinPlayers) return new();
    //
    //     var c = q.Criteria?.Deserialize<MatchingCriteria>();
    //     if (c == null || c.Attributes == null || c.Attributes.Count == 0)
    //         return tickets.Take(Math.Min(q.MaxPlayers, tickets.Count)).ToList();
    //
    //     var used = new HashSet<Guid>();
    //     foreach (var anchor in tickets)
    //     {
    //         if (used.Contains(anchor.PlayerId)) continue;
    //         var anchorAttrs =
    //             await _playerAttributeRepository.GetMatchingAttributesAsync(anchor.PlayerId, c.Attributes);
    //         var possible = new List<(MatchTicket Ticket, double Score)>();
    //
    //         foreach (var candidate in tickets.Where(x => x.Id != anchor.Id && !used.Contains(x.PlayerId)))
    //         {
    //             var candAttrs =
    //                 await _playerAttributeRepository.GetMatchingAttributesAsync(candidate.PlayerId, c.Attributes);
    //             var score = CalculateScore(anchorAttrs, candAttrs, c.Attributes, anchor.CreatedAt);
    //             if (score > 0) possible.Add((candidate, score));
    //         }
    //
    //         if (possible.Count >= q.MinPlayers - 1)
    //         {
    //             var best = possible.OrderByDescending(x => x.Score)
    //                 .Take(Math.Min(q.MaxPlayers - 1, possible.Count))
    //                 .ToList();
    //             var group = new List<MatchTicket> { anchor };
    //             group.AddRange(best.Select(x => x.Ticket));
    //             used.Add(anchor.PlayerId);
    //             foreach (var b in best) used.Add(b.Ticket.PlayerId);
    //             if (group.Count >= q.MinPlayers) return group;
    //         }
    //     }
    //
    //     if (tickets.Count >= q.MinPlayers && used.Count == 0)
    //         return tickets.Take(Math.Min(q.MaxPlayers, tickets.Count)).ToList();
    //
    //     return new();
    // }
    //
    // private double CalculateScore(IEnumerable<PlayerAttribute> anchorAttrs, IEnumerable<PlayerAttribute> candAttrs,
    //     List<AttributeCriteria> criteria, DateTime anchorCreated)
    // {
    //     double total = 0, max = criteria.Count * 100;
    //     foreach (var c in criteria)
    //     {
    //         var a = anchorAttrs.FirstOrDefault(x => x.Collection == c.Collection && x.Key == c.Key);
    //         var cd = candAttrs.FirstOrDefault(x => x.Collection == c.Collection && x.Key == c.Key);
    //         if (a == null || cd == null) continue;
    //         total += ScoreCriterion(a.Value, cd.Value, c, anchorCreated);
    //     }
    //
    //     return total / max;
    // }
    //
    // private double ScoreCriterion(string anchorVal, string candVal, AttributeCriteria c, DateTime anchorCreated)
    // {
    //     if (double.TryParse(anchorVal, out var aNum) && double.TryParse(candVal, out var cNum))
    //     {
    //         if (c.Range != null)
    //         {
    //             var waitFactor = Math.Min(1.0 + (DateTime.UtcNow - anchorCreated).TotalMinutes * 0.1, 2.0);
    //             var allowed = c.Range.MaxExpansion ?? (c.Range.ExpansionRate * waitFactor);
    //             var diff = Math.Abs(aNum - cNum);
    //             if (diff <= allowed) return 100 * (1 - diff / allowed);
    //         }
    //         else
    //         {
    //             var op = c.Operator.ToLower();
    //             if (op == "eq" && aNum == cNum) return 100;
    //             if (op == "lt" && cNum < aNum) return 100;
    //             if (op == "lte" && cNum <= aNum) return 100;
    //             if (op == "gt" && cNum > aNum) return 100;
    //             if (op == "gte" && cNum >= aNum) return 100;
    //         }
    //     }
    //     else
    //     {
    //         var op = c.Operator.ToLower();
    //         if (op == "eq" && anchorVal == candVal) return 100;
    //         if (op == "contains" && candVal.Contains(anchorVal)) return 100;
    //         if (op == "startswith" && candVal.StartsWith(anchorVal)) return 100;
    //         if (op == "endswith" && candVal.EndsWith(anchorVal)) return 100;
    //     }
    //
    //     return 0;
    // }
    //
    // #endregion
    //
    // public async Task<MatchResponse?> CheckMatchStatusAsync(Guid ticketId)
    // {
    //     var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    //     if (ticket?.MatchId == null) return null;
    //     var match = await _matchRepository.GetByIdAsync(ticket.MatchId.Value);
    //     return match == null ? null : _mapper.Map<MatchResponse>(match);
    // }
    public Task<MatchmakingResponse> CreateQueueAsync(MatchQueueRequest queueRequest)
    {
        throw new NotImplementedException();
    }

    public Task<MatchmakingResponse> UpdateQueueAsync(Guid queueId, MatchQueueRequest queueRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteQueueAsync(Guid queueId)
    {
        throw new NotImplementedException();
    }

    public Task<MatchmakingResponse?> GetQueueAsync(Guid? queueId = null, Guid? gameId = null, string? queueName = null)
    {
        throw new NotImplementedException();
    }

    public Task<MatchTicketResponse> CreateTicketAsync(Guid gameId, Guid playerId, string queueName, JsonDocument? matchCriteria = null,
        JsonDocument? properties = null)
    {
        throw new NotImplementedException();
    }

    public Task<MatchResponse> FindMatchAsync(FindMatchRequest request, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task CancelTicketAsync(Guid ticketId)
    {
        throw new NotImplementedException();
    }

    public Task<MatchTicketResponse?> GetTicketAsync(Guid ticketId)
    {
        throw new NotImplementedException();
    }

    public Task<List<MatchResponse>> ProcessMatchmakingAsync(Guid? queueId = null)
    {
        throw new NotImplementedException();
    }

    public Task AcceptMatchAsync(Guid ticketId)
    {
        throw new NotImplementedException();
    }

    public Task DeclineMatchAsync(Guid ticketId)
    {
        throw new NotImplementedException();
    }

    public Task<MatchResponse?> GetMatchAsync(Guid matchId)
    {
        throw new NotImplementedException();
    }

    public Task<MatchResponse?> CheckMatchStatusAsync(Guid ticketId)
    {
        throw new NotImplementedException();
    }

    public Task CancelMatchAsync(Guid matchId)
    {
        throw new NotImplementedException();
    }

    public Task<MatchResponse> UpdateMatchStateAsync(Guid matchId, JsonDocument playerStates, JsonDocument matchState, Guid? nextPlayerId = null,
        DateTime? nextDeadline = null)
    {
        throw new NotImplementedException();
    }

    public Task<MatchActionResponse> SubmitActionAsync(Guid matchId, Guid playerId, MatchActionRequest action)
    {
        throw new NotImplementedException();
    }

    public Task<List<MatchActionResponse>> GetMatchActionsAsync(Guid matchId, DateTime? since = null, int? limit = null)
    {
        throw new NotImplementedException();
    }

    public Task<JsonDocument> GetMatchStateAsync(Guid matchId)
    {
        throw new NotImplementedException();
    }

    public Task<JsonDocument> GetPlayerStateAsync(Guid matchId, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task<List<MatchActionResponse>> GetTurnHistoryAsync(Guid matchId)
    {
        throw new NotImplementedException();
    }

    public Task<List<MatchResponse>> GetPlayerActiveMatchesAsync(Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task<List<MatchTicketResponse>> GetPlayerActiveTicketsAsync(Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidatePlayerTurnAsync(Guid matchId, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task ProcessTurnTimeoutsAsync()
    {
        throw new NotImplementedException();
    }
}