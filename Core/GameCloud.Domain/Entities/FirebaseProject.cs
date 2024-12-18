namespace GameCloud.Domain.Entities;

public class FirebaseProject : BaseEntity
{
    public Guid GameId { get; set; }
    public string? FirebaseProjectId { get; set; }
    public string? ApiKey { get; set; }
}