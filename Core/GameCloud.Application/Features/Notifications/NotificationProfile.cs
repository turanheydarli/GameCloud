using AutoMapper;
using GameCloud.Application.Features.Notifications.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Notifications;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationResponse>();
    }
}