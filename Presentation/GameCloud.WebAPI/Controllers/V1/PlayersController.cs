using GameCloud.Application.Common.Paging;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Application.Features.Notifications;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Users;
using GameCloud.Domain.Entities;
using GameCloud.WebAPI.Filters.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class PlayersController(
    IPlayerService playerService,
    INotificationService notificationService,
    IUserService userService)
    : BaseController
{
    [HttpPost("authenticate/device")]
    [RequireGameKey]
    public async Task<IActionResult> AuthenticateDevice(
        [FromBody] DeviceAuthRequest request)
    {
        var response = await playerService.AuthenticateWithDeviceAsync(
            request.DeviceId,
            request.Metadata);

        return Ok(response);
    }

    [HttpPost("authenticate/custom")]
    [RequireGameKey]
    public async Task<IActionResult> AuthenticateCustom(
        [FromBody] CustomAuthRequest request)
    {
        var response = await playerService.AuthenticateWithCustomIdAsync(
            request.CustomId,
            request.Metadata,
            request.Create);

        return Ok(response);
    }

    [HttpPost("authenticate/refresh")]
    [RequireGameKey]
    public async Task<IActionResult> RefreshSession(
        [FromBody] SessionRefreshRequest request)
    {
        var response = await playerService.RefreshSessionAsync(request.SessionId);
        return Ok(response);
    }


    [HttpGet("me")]
    [Authorize(Policy = "HasGameKey", Roles = "Player")]
    public async Task<IActionResult> GetMe()
    {
        var playerId = GetPlayerIdFromClaims();
        return Ok(await playerService.GetByIdAsync(playerId));
    }

    [HttpPut("me")]
    [Authorize(Policy = "HasGameKey", Roles = "Player")]
    public async Task<IActionResult> Update(PlayerRequest player)
    {
        var playerId = GetPlayerIdFromClaims();
        return Ok(await playerService.UpdateAsync(playerId, player));
    }

    [RequireGameKey]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Auth(AuthPlayerRequest request)
    {
        return Ok(await userService.AuthenticatePlayerAsync(request));
    }

    [HttpGet("{username}/notifications")]
    public async Task<IActionResult> GetPlayerNotifications(
        string username,
        [FromQuery] PageableRequest request,
        [FromQuery] NotificationStatus status = NotificationStatus.Sent)
    {
        var notifications = await notificationService.GetPlayerNotificationsAsync(
            username, status, request);
        return Ok(notifications);
    }

    [HttpGet("{username}/attributes/{collection}")]
    public async Task<IActionResult> GetAttributes(string username, string collection)
    {
        var attributes = await playerService.GetAttributesAsync(collection, username);
        return Ok(attributes);
    }

    [HttpGet("{username}/attributes/{collection}/{key}")]
    public async Task<IActionResult> GetAttribute(
        string username,
        string collection,
        string key)
    {
        var attribute = await playerService.GetAttributeAsync(username, collection, key);
        return Ok(attribute);
    }


    [HttpPut("{username}/attributes/{collection}")]
    public async Task<IActionResult> SetAttribute(
        string username,
        string collection,
        [FromBody] AttributeRequest request)
    {
        await playerService.SetAttributeAsync(username, collection, request);
        return Ok();
    }

    [HttpDelete("{username}/attributes/{collection}/{key}")]
    public async Task<IActionResult> DeleteAttribute(
        string username,
        string collection,
        string key)
    {
        await playerService.RemoveAttributeAsync(username, collection, key);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "HasGameKey")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var player = await playerService.GetByIdAsync(id);
        if (player == null)
            return NotFound();

        return Ok(player);
    }
}

public class DeviceAuthRequest
{
    public string DeviceId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CustomAuthRequest
{
    public string CustomId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public bool Create { get; set; } = true;
}

public class SessionRefreshRequest
{
    public string SessionId { get; set; }
}