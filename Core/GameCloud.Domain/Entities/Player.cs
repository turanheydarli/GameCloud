using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class Player : BaseEntity
{
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Attached ID from client side durning first session start or regstiration.
    /// </summary>
    public string PlayerId { get; set; }
    public Guid SessionId { get; set; }
    public PlayerStatus Status { get; set; }
    public AuthProvider AuthProvider { get; set; }

    public AppUser User { get; set; }
}