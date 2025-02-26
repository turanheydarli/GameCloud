using System.Text.Json;

namespace GameCloud.Domain.Entities.Leaderboards
{
    public enum LeaderboardSortOrder
    {
        Ascending,
        Descending
    }

    public enum LeaderboardOperator
    {
        Set,
        Increment,
        Maximum,
        Minimum,
        Latest,
        Average
    }

    public enum ResetFrequency
    {
        Never,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Custom
    }

    public class LeaderboardResetStrategy
    {
        public ResetFrequency Frequency { get; set; }
        public DayOfWeek? ResetDay { get; set; }
        public int? ResetDayOfMonth { get; set; }
        public TimeSpan? ResetTime { get; set; }
        public bool ArchiveScores { get; set; }
        public int? ArchiveRetentionMonths { get; set; }
    }

    public class ValidationRules
    {
        public long? MinScore { get; set; }
        public long? MaxScore { get; set; }
        public int? MaxScoreIncrease { get; set; }
        public TimeSpan? MinGameDuration { get; set; }
        public List<string>? RequiredMetadataFields { get; set; }
    }

    public class LeaderboardConfig
    {
        public bool AllowTies { get; set; }
        public TimeSpan? MinScoreInterval { get; set; }
        public int? MaxDailySubmissions { get; set; }
        public bool RequireAuthentication { get; set; }
        public List<string>? AllowedCountries { get; set; }
        public ValidationRules? ValidationRules { get; set; }
    }

    public class Leaderboard : BaseEntity
    {
        public Guid GameId { get; set; }
        public string Name { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? Description { get; set; }
        public LeaderboardSortOrder SortOrder { get; set; }
        public LeaderboardOperator Operator { get; set; }
        public LeaderboardResetStrategy? ResetStrategy { get; set; }
        public int? MaxSize { get; set; }
        public JsonDocument? Metadata { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public virtual Game Game { get; set; } = default!;
        public virtual ICollection<LeaderboardRecord> Records { get; set; } = new List<LeaderboardRecord>();
        public virtual ICollection<LeaderboardArchive> Archives { get; set; } = new List<LeaderboardArchive>();
    }

    public class LeaderboardRecord : BaseEntity
    {
        public Guid LeaderboardId { get; set; }
        public Guid PlayerId { get; set; }
        public long Score { get; set; }
        public int? Rank { get; set; }
        public long SubScore { get; set; }
        public int UpdateCount { get; set; }
        public JsonDocument? Metadata { get; set; }
        public virtual Leaderboard Leaderboard { get; set; } = default!;
        public virtual Player Player { get; set; } = default!;
    }
}
