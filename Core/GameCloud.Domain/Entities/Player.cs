using System.Text.Json;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class Player : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string CustomId { get; set; }

    public Guid GameId { get; set; }
    public PlayerStatus Status { get; set; }
    public AuthProvider AuthProvider { get; set; }

    public virtual AppUser? User { get; set; }

    public virtual ICollection<PlayerAttribute> Attributes { get; set; }
    public string DeviceId { get; set; }
    public JsonDocument Metadata { get; set; }
    public DateTime LastLoginAt { get; set; }
}