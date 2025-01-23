using System.Text.Json;

namespace GameCloud.Application.Features.Leaderboards.Requests;

public class LeaderboardRequest
{
    public string Name { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }
    public int? MaxSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public JsonDocument? Metadata { get; set; }
}