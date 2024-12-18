namespace GameCloud.Domain.Entities;

public class Developer : BaseEntity
{
    public Guid UserId { get; set; }
    
    public string? Name { get; set; }
    public string? Email { get; set; }
    
    public ICollection<Game> Games { get; set; }
}