using GameCloud.Application.Features.Notifications.Responses;

namespace GameCloud.Application.Features.Notifications;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetPlayerNotificationsAsync(Guid playerId, string status);
}