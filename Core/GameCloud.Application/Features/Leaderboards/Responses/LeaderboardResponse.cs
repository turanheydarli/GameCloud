using System.Text.Json;

namespace GameCloud.Application.Features.Leaderboards.Responses;

public class LeaderboardResponse
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }
    public int? MaxSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class LeaderboardRecordResponse
{
    public Guid LeaderboardId { get; set; }
    public Guid PlayerId { get; set; }
    public long Score { get; set; }
    public int? Rank { get; set; }
    public long SubScore { get; set; }
    public JsonDocument? Metadata { get; set; }
}

public class LeaderboardArchiveResponse
{
    public Guid ArchiveId { get; set; }
    public Guid LeaderboardId { get; set; }
    public string Period { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public JsonDocument Rankings { get; set; } = default!;
    public JsonDocument? Statistics { get; set; }
}