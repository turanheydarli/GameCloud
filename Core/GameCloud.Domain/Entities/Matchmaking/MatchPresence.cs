using System.Text.Json;

namespace GameCloud.Domain.Entities.Matchmaking;

public class MatchPresence
{
    public Guid PlayerId { get; set; }
    public string SessionId { get; set; }
    public string Status { get; set; }
    public string Mode { get; set; } // "player", "spectator", "host"
    public DateTime JoinedAt { get; set; }
    public DateTime LastSeen { get; set; }
    public JsonDocument Meta { get; set; }
    public bool Hidden { get; set; } // For invisible spectators
}