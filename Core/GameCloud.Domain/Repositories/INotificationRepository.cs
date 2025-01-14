using GameCloud.Domain.Entities;

namespace GameCloud.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task CreateManyAsync(IEnumerable<Notification> notifications);

    Task<IPaginate<Notification>> GetNotificationsByPlayerAsync(
        string username,
        NotificationStatus status,
        int pageIndex,
        int pageSize
    );
}