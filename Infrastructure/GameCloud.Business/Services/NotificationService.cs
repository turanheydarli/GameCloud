using AutoMapper;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Notifications.Responses;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class NotificationService(INotificationRepository notificationRepository, IMapper mapper) : INotificationService
{
    public async Task<List<NotificationResponse>> GetPlayerNotificationsAsync(Guid playerId, string status)
    {
        var notifications = await notificationRepository.GetNotificationsByPlayerAsync(playerId, status);

        return mapper.Map<List<NotificationResponse>>(notifications);
    }
}