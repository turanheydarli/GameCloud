using System.Text.Json;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class Player : BaseEntity
{
    public Guid UserId { get; set; }
    public string PlayerId { get; set; }
    public Guid GameId { get; set; }
    public PlayerStatus Status { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public JsonDocument Attributes { get; set; } = JsonDocument.Parse("{}");
    
    public virtual AppUser User { get; set; }
}