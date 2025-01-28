using System.Text.Json;
public class MatchState
{
    public MatchStateStatus Status { get; set; }
    public MatchPhase Phase { get; set; }
    public DateTime? StartedAt { get; set; }
    public JsonDocument GameState { get; set; }
    public JsonDocument Metadata { get; set; }
    public List<PresenceState> Presences { get; set; } = new();
    public int Size { get; set; }
    public string Label { get; set; }
    public int TickRate { get; set; } = 1;
}
public enum MatchStateStatus
{
    Created,
    Joining,
    Ready,
    Active,
    Suspended,
    Completed,
    Abandoned
}
public enum MatchPhase
{
    Initialization,
    WaitingPlayers,
    ReadyCheck,
    Starting,
    Playing,
    Paused,
    GameOver,
    Cleanup
}
public enum PresenceStatus
{
    Connected,
    Disconnected,
    Joining,
    Left,
    Spectating,
    Away
}

public class PresenceState
{
    public string PlayerId { get; set; }
    public string SessionId { get; set; }
    public PresenceStatus Status { get; set; }
    public JsonDocument Meta { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsConnected => Status == PresenceStatus.Connected;
    public string Mode { get; set; } = "player";
}