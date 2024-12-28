using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications.Requests;

public record NotificationRequest(
    Guid Id,
    Guid From,
    Guid To,
    Guid SubscriptionId,
    Guid SessionId,
    Guid? ActionId,
    Dictionary<string, object> Data,
    NotificationChannel Channel,
    DateTime SentAt,
    NotificationStatus Status,
    string? Title,
    string? Body,
    string? Icon
);