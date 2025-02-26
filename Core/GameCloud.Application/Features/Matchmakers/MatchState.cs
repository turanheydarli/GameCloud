using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Application.Features.Matchmakers;

public class PlayerMatchProperties
{
    public bool IsStoredPlayer { get; set; }
    public Guid StoredMatchId { get; set; }
    public PlayerMatchStatistics? Statistics { get; set; }
    public DateTime LastPlayedAt { get; set; }
    public string Mode { get; set; } = "player";
    public JsonDocument GameData { get; set; }
}

public class PlayerMatchStatistics
{
    public int Score { get; set; }
    public int Moves { get; set; }
    public TimeSpan PlayTime { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRate { get; set; }
    public Dictionary<string, int> GameSpecificStats { get; set; } = new();
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBot { get; set; }
}

public class MatchState
{
    [JsonPropertyName("status")] public MatchStateStatus Status { get; set; }

    [JsonPropertyName("phase")] public MatchPhase Phase { get; set; }

    [JsonPropertyName("startedAt")] public DateTime? StartedAt { get; set; }

    [JsonPropertyName("matchType")] public string MatchType { get; set; } = "standard";

    [JsonPropertyName("finalScore")] public int FinalScore { get; set; }

    [JsonPropertyName("gameState")] public JsonDocument GameState { get; set; }

    [JsonPropertyName("metadata")] public JsonDocument Metadata { get; set; }

    [JsonPropertyName("presences")] public List<PresenceState> Presences { get; set; } = new();

    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("label")] public string Label { get; set; }

    [JsonPropertyName("tickRate")] public int TickRate { get; set; }

    [JsonPropertyName("duration")] public double Duration { get; set; }

    [JsonPropertyName("matchQuality")] public double MatchQuality { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchStateStatus
{
    [JsonPropertyName("created")] Created,

    [JsonPropertyName("joining")] Joining,

    [JsonPropertyName("ready")] Ready,

    [JsonPropertyName("active")] Active,

    [JsonPropertyName("suspended")] Suspended,

    [JsonPropertyName("completed")] Completed,

    [JsonPropertyName("abandoned")] Abandoned
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchPhase
{
    [JsonPropertyName("initialization")] Initialization,

    [JsonPropertyName("waitingPlayers")] WaitingPlayers,

    [JsonPropertyName("readyCheck")] ReadyCheck,

    [JsonPropertyName("starting")] Starting,

    [JsonPropertyName("playing")] Playing,

    [JsonPropertyName("paused")] Paused,

    [JsonPropertyName("gameOver")] GameOver,

    [JsonPropertyName("cleanup")] Cleanup
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PresenceStatus
{
    [JsonPropertyName("connected")] Connected,

    [JsonPropertyName("disconnected")] Disconnected,

    [JsonPropertyName("joining")] Joining,

    [JsonPropertyName("left")] Left,

    [JsonPropertyName("spectating")] Spectating,

    [JsonPropertyName("away")] Away
}

public class PresenceState
{
    [JsonPropertyName("playerId")] public string PlayerId { get; set; }

    [JsonPropertyName("sessionId")] public string SessionId { get; set; }

    [JsonPropertyName("status")] public PresenceStatus Status { get; set; }

    [JsonPropertyName("meta")] public JsonDocument Meta { get; set; }

    [JsonPropertyName("lastSeen")] public DateTime LastSeen { get; set; }

    [JsonPropertyName("joinedAt")] public DateTime JoinedAt { get; set; }

    [JsonIgnore] public bool IsConnected => Status == PresenceStatus.Connected;

    [JsonPropertyName("mode")] public string Mode { get; set; } = "player";
}