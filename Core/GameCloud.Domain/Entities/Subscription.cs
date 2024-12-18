using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string? FcmToken { get; set; }
    public SubscriptionPlatform Platform { get; set; }
}