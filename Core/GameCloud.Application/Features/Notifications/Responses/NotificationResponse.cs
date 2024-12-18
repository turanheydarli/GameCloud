using System.Text.Json;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications.Responses;

public record NotificationResponse(
    Guid Id,
    string? Title,
    string? Body,
    string? Icon,
    JsonDocument? Data,
    NotificationStatus Status,
    DateTime SentAt,
    Guid? ActionId,
    Guid PlayerId,
    Guid? SessionId
);