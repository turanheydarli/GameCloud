using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Notifications.Requests;
using GameCloud.Application.Features.Notifications.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications;

public interface INotificationService
{
    Task RegisterNotificationList(IEnumerable<NotificationRequest> notifications);
    Task<PageableListResponse<NotificationResponse>> GetPlayerNotificationsAsync(Guid playerId, NotificationStatus status, PageableRequest request);
}