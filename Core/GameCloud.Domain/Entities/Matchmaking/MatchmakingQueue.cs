using System.Text.Json;

namespace GameCloud.Domain.Entities.Matchmaking;

public class MatchmakingQueue : BaseEntity
{
    public Guid GameId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }

    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public TimeSpan TicketTTL { get; set; }
    public TimeSpan? TurnTimeout { get; set; }
    public TimeSpan? MatchTimeout { get; set; }

    public JsonDocument Criteria { get; set; }
    public JsonDocument Rules { get; set; }

    public virtual Game Game { get; set; }
}

public record MatchingCriteria(List<AttributeCriteria> Attributes);

public record AttributeCriteria(
    string Collection,
    string Key,
    string Operator,
    RangeCriteria? Range = null);

public record RangeCriteria(
    double ExpansionRate,
    double? MaxExpansion);

public class MatchTicket : BaseEntity
{
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public string QueueName { get; set; }
    public TicketStatus Status { get; set; }

    public JsonDocument MatchCriteria { get; set; }
    public JsonDocument Properties { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? MatchId { get; set; }

    public virtual Player Player { get; set; }
    public virtual Match Match { get; set; }
}

public class Match : BaseEntity
{
    public Guid GameId { get; set; }
    public string QueueName { get; set; }
    public MatchState State { get; set; }

    public List<Guid> PlayerIds { get; set; }
    public Guid? CurrentPlayerId { get; set; }
    public JsonDocument PlayerStates { get; set; }
    public JsonDocument MatchState { get; set; }
    public JsonDocument TurnHistory { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? LastActionAt { get; set; }
    public DateTime? NextActionDeadline { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<MatchTicket> Tickets { get; set; }
    public virtual ICollection<MatchAction> Actions { get; set; }
}

public class MatchAction : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActionType { get; set; }
    public JsonDocument ActionData { get; set; }

    public virtual Match Match { get; set; }
    public virtual Player Player { get; set; }
}

public enum TicketStatus
{
    /// <summary>
    /// Ticket is created and waiting in queue
    /// </summary>
    Queued = 0,

    /// <summary>
    /// Potential match is found, validating requirements
    /// </summary>
    Matching = 1,

    /// <summary>
    /// Match is found and waiting for player response
    /// </summary>
    MatchFound = 2,

    /// <summary>
    /// Player accepted the match
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// Player declined the match
    /// </summary>
    Declined = 4,

    /// <summary>
    /// Ticket is cancelled by player
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Ticket expired due to timeout
    /// </summary>
    Expired = 6,

    /// <summary>
    /// Match is successfully created
    /// </summary>
    Matched = 7,

    /// <summary>
    /// Error occurred during matchmaking
    /// </summary>
    Error = 8
}

public enum MatchState
{
    /// <summary>
    /// Match is created but not all players accepted yet
    /// </summary>
    Created = 0,

    /// <summary>
    /// All players accepted, match is ready to start
    /// </summary>
    Ready = 1,

    /// <summary>
    /// Match is in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Waiting for next player's turn
    /// </summary>
    WaitingTurn = 3,

    /// <summary>
    /// Match finished normally
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Match cancelled due to player decline/timeout
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Match abandoned by player(s)
    /// </summary>
    Abandoned = 6,

    /// <summary>
    /// Match expired due to inactivity
    /// </summary>
    Expired = 7,

    /// <summary>
    /// Error occurred during match
    /// </summary>
    Error = 8
}