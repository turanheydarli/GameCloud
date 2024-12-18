namespace GameCloud.Domain.Entities;

public class Game : BaseEntity
{
    public Guid DeveloperId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    
    public Developer? Developer { get; set; }
    public ICollection<GameKey> GameKeys { get; set; }
}