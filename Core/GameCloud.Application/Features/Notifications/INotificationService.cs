using GameCloud.Domain.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Notifications.Requests;
using GameCloud.Application.Features.Notifications.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications;

public interface INotificationService
{
    Task RegisterNotificationList(IEnumerable<NotificationRequest> notifications);
    Task<PageableListResponse<NotificationResponse>> GetPlayerNotificationsAsync(string username, NotificationStatus status, PageableRequest request);
}