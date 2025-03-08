namespace GameCloud.Domain.Entities.Rooms;

public class Room : BaseEntity
{
    public string Name { get; set; }
    public string GameId { get; set; }
    public Guid CreatorId { get; set; }
    public RoomState State { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<string> PlayerIds { get; set; } = new();
    public List<string> SpectatorIds { get; set; } = new();
    public int MaxPlayers { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CurrentTurnUserId { get; set; }
    public int TurnNumber { get; set; }
    public bool IsPrivate { get; set; }
    public string Label { get; set; }
    
    public RoomConfig Config { get; set; }
}

public enum RoomState
{
    Unspecified = 0,
    Created = 1,
    Started = 2,
    Ended = 3,
    Closed = 4
}