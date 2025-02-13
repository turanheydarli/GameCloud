using System.Text.Json;

namespace GameCloud.Domain.Entities.Matchmaking;

public class StoredMatch : BaseEntity
{
    public Guid OriginalMatchId { get; set; }
    public Guid GameId { get; set; }
    public string QueueName { get; set; }
    public string MatchType { get; set; }
    public int FinalScore { get; set; }
    public double Duration { get; set; }
    public double MatchQuality { get; set; }
    public string Label { get; set; }
    public int Size { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool IsAvailableForMatching { get; set; }
    public JsonDocument Metadata { get; set; }
    public List<StoredPlayer> Players { get; set; } = new();
    public JsonDocument GameState { get; set; }
}

public class StoredPlayer : BaseEntity
{
    public JsonDocument Actions { get; set; }
    public JsonDocument Statistics { get; set; }
    public DateTime LastPlayedAt { get; set; }
    public string Mode { get; set; } = "player";
    public Guid StoredMatchId { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
}