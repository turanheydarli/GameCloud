using System.Text.Json;

namespace GameCloud.Domain.Entities.Leaderboards;

public class LeaderboardArchive : BaseEntity
{
    public Guid LeaderboardId { get; set; }
    public string Period { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public JsonDocument Rankings { get; set; } = default!;
    public JsonDocument? Statistics { get; set; }
    public virtual Leaderboard Leaderboard { get; set; } = default!;
}