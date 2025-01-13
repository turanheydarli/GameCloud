using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications;

public interface INotificationDispatcher
{
    Task DispatchAsync(Notification notification);
}