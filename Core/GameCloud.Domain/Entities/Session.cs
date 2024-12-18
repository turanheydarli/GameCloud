using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class Session : BaseEntity
{
    public Guid GameId { get; set; }
    public SessionStatus Status { get; set; }
}