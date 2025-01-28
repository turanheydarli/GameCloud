using System.Text.Json;

namespace GameCloud.Domain.Entities.Matchmaking;

public class MatchPresenceState
{
    public Guid PlayerId { get; set; }
    public string SessionId { get; set; }
    public string Status { get; set; }
    public string Mode { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime LastSeen { get; set; }
    public JsonDocument Meta { get; set; }
}