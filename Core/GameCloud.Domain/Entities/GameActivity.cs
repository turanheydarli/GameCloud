namespace GameCloud.Domain.Entities;

public class GameActivity : BaseEntity
{
    public Guid GameId { get; set; }
    public Game Game { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}