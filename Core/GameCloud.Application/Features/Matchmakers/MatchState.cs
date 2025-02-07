using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Application.Features.Matchmakers;

public class MatchState
{
    [JsonPropertyName("status")]
    public MatchStateStatus Status { get; set; }

    [JsonPropertyName("phase")]
    public MatchPhase Phase { get; set; }

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("gameState")]
    public JsonDocument GameState { get; set; }

    [JsonPropertyName("metadata")]
    public JsonDocument Metadata { get; set; }

    [JsonPropertyName("presences")]
    public List<PresenceState> Presences { get; set; } = new();

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("tickRate")]
    public int TickRate { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchStateStatus
{
    [JsonPropertyName("created")]
    Created,
    
    [JsonPropertyName("joining")]
    Joining,
    
    [JsonPropertyName("ready")]
    Ready,
    
    [JsonPropertyName("active")]
    Active,
    
    [JsonPropertyName("suspended")]
    Suspended,
    
    [JsonPropertyName("completed")]
    Completed,
    
    [JsonPropertyName("abandoned")]
    Abandoned
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchPhase
{
    [JsonPropertyName("initialization")]
    Initialization,
    
    [JsonPropertyName("waitingPlayers")]
    WaitingPlayers,
    
    [JsonPropertyName("readyCheck")]
    ReadyCheck,
    
    [JsonPropertyName("starting")]
    Starting,
    
    [JsonPropertyName("playing")]
    Playing,
    
    [JsonPropertyName("paused")]
    Paused,
    
    [JsonPropertyName("gameOver")]
    GameOver,
    
    [JsonPropertyName("cleanup")]
    Cleanup
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PresenceStatus
{
    [JsonPropertyName("connected")]
    Connected,
    
    [JsonPropertyName("disconnected")]
    Disconnected,
    
    [JsonPropertyName("joining")]
    Joining,
    
    [JsonPropertyName("left")]
    Left,
    
    [JsonPropertyName("spectating")]
    Spectating,
    
    [JsonPropertyName("away")]
    Away
}

public class PresenceState
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; }

    [JsonPropertyName("status")]
    public PresenceStatus Status { get; set; }

    [JsonPropertyName("meta")]
    public JsonDocument Meta { get; set; }

    [JsonPropertyName("lastSeen")]
    public DateTime LastSeen { get; set; }

    [JsonPropertyName("joinedAt")]
    public DateTime JoinedAt { get; set; }

    [JsonIgnore]  
    public bool IsConnected => Status == PresenceStatus.Connected;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "player";
}