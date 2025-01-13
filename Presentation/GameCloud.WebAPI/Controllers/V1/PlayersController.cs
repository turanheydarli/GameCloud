using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Users;
using GameCloud.Application.Common.Requests;
using GameCloud.Domain.Entities;
using GameCloud.WebAPI.Filters.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class PlayersController(
    IPlayerService playerService,
    INotificationService notificationService,
    IUserService userService) : BaseController
{
    [HttpGet("me")]
    [Authorize(Policy = "HasGameKey", Roles = "Player")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();

        return Ok(await playerService.GetByUserIdAsync(userId));
    }

    [RequireGameKey]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Auth(AuthPlayerRequest request)
    {
        return Ok(await userService.AuthenticatePlayerAsync(request));
    }

    [HttpGet("{userId}/notifications")]
    public async Task<IActionResult> GetPlayerNotifications(Guid userId, [FromQuery] PageableRequest request,
        [FromQuery] NotificationStatus status = NotificationStatus.Sent)
    {
        var notifications = await notificationService.GetPlayerNotificationsAsync(userId, status, request);

        return Ok(notifications);
    }

    [HttpGet("{playerId}/attributes")]
    public async Task<IActionResult> GetAttributes()
    {
        var userId = GetUserIdFromClaims();
        return Ok(await playerService.GetAttributesAsync(userId));
    }

    [HttpGet("{playerId}/attributes/{key}")]
    public async Task<IActionResult> GetAttribute(string key)
    {
        var userId = GetUserIdFromClaims();
        return Ok(await playerService.GetAttributeAsync(userId, key));
    }

    [HttpPost("{playerId}/attributes")]
    public async Task<IActionResult> CreateAttribute(AttributeRequest request)
    {
        var userId = GetUserIdFromClaims();
        await playerService.SetAttributeAsync(userId, request);
        return Created();
    }
}