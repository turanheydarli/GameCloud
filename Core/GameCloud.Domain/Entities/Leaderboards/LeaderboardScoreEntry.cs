using System.Text.Json;

namespace GameCloud.Domain.Entities.Leaderboards;

public class LeaderboardScoreEntry : BaseEntity
{
    public Guid LeaderboardId { get; set; }
    public Guid PlayerId { get; set; }
    public long NewScore { get; set; }
    public long PreviousScore { get; set; }
    public JsonDocument? Metadata { get; set; }
    public string? ClientIp { get; set; }
    public string? SessionId { get; set; }
    public virtual Leaderboard Leaderboard { get; set; } = default!;
    public virtual Player Player { get; set; } = default!;
}