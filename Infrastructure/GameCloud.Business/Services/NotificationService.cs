using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Notifications.Requests;
using GameCloud.Application.Features.Notifications.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IPlayerRepository playerRepository,
    IMapper mapper) : INotificationService
{
    public async Task RegisterNotificationList(IEnumerable<NotificationRequest> notifications)
    {
        var notificationEntities = new List<Notification>();

        foreach (var request in notifications)
        {
            var fromPlayer = await playerRepository.GetByUserIdAsync(request.From);
            if (fromPlayer is null)
            {
                continue;
            }

            var toPlayer = await playerRepository.GetByUserIdAsync(request.To);
            if (toPlayer is null)
            {
                continue;
            }

            var entity = new Notification
            {
                SubscriptionId = request.SubscriptionId,
                SessionId = request.SessionId,
                ActionId = request.ActionId,
                To = request.To,
                From = request.From,
                Title = request.Title,
                Body = request.Body,
                Icon = request.Icon,
                Channel = request.Channel,
                SentAt = request.SentAt == default ? DateTime.UtcNow : request.SentAt,
                Status = request.Status,
                Data = JsonDocument.Parse(JsonSerializer.Serialize(request.Data)),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            notificationEntities.Add(entity);
        }

        if (notificationEntities.Any())
        {
            await notificationRepository.CreateManyAsync(notificationEntities);
        }
    }


    public async Task<PageableListResponse<NotificationResponse>> GetPlayerNotificationsAsync(string username,
        NotificationStatus status, PageableRequest request)
    {
        var notifications =
            await notificationRepository.GetNotificationsByPlayerAsync(username, status, request.PageIndex,
                request.PageSize);

        return mapper.Map<PageableListResponse<NotificationResponse>>(notifications);
    }
}