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
}