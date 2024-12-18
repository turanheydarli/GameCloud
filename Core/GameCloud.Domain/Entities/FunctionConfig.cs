namespace GameCloud.Domain.Entities;

public class FunctionConfig : BaseEntity
{
    public Guid GameId { get; set; }
    public string Name { get; set; }
    public string ActionType { get; set; }
    public string Endpoint { get; set; }
    public bool IsEnabled { get; set; }
}