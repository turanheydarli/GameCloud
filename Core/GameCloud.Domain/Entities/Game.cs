namespace GameCloud.Domain.Entities;

public class Game : BaseEntity
{
    public Guid DeveloperId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
    public Guid? ImageId { get; set; }
    public virtual ImageDocument? Image { get; set; }
    public virtual Developer? Developer { get; set; }
    public virtual ICollection<GameKey>? GameKeys { get; set; }
    
    public ICollection<Player>? Players { get; set; } = new List<Player>();
    public ICollection<FunctionConfig>? Functions { get; set; } = new List<FunctionConfig>();
    public ICollection<GameActivity>? Activities { get; set; } = new List<GameActivity>();
}