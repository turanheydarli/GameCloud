using GameCloud.Application.Features.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class PlayersController(INotificationService notificationService) : BaseController
{
    [HttpGet("{playerId}/notifications")]
    public async Task<IActionResult> GetPlayerNotifications(Guid playerId, [FromQuery] string status = "unread")
    {
        var notifications = await notificationService.GetPlayerNotificationsAsync(playerId, status);

        return Ok(notifications);
    }
}