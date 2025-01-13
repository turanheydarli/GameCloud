using System.Text.Json;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications.Responses;

public record NotificationResponse(
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