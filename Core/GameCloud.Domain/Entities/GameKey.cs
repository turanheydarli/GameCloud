namespace GameCloud.Domain.Entities;

public enum GameKeyStatus
{
    Active = 1,
    Revoked = 2,
}

public class GameKey : BaseEntity
{
    public Guid GameId { get; set; }
    public string ApiKey { get; set; }
    public GameKeyStatus Status { get; set; }
    
    public Game Game { get; set; }
}