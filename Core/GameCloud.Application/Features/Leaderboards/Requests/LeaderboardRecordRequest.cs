using System.Text.Json;

namespace GameCloud.Application.Features.Leaderboards.Requests;

public class LeaderboardRecordRequest
{
    public Guid PlayerId { get; set; }
    public long Score { get; set; }
    public long SubScore { get; set; }
    public JsonDocument? Metadata { get; set; }
    public Guid LeaderboardId { get; set; }
}