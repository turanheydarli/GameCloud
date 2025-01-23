using System.Text.Json;

namespace GameCloud.Application.Features.Leaderboards.Requests;

public class LeaderboardScoreRequest
{
    public Guid LeaderboardId { get; set; }
    public long Score { get; set; }
    public JsonDocument? Metadata { get; set; }
}