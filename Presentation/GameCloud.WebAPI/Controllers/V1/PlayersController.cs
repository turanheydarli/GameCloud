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
public class PlayersController : BaseController
{
    private readonly IPlayerService _playerService;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    public PlayersController(
        IPlayerService playerService,
        INotificationService notificationService,
        IUserService userService)
    {
        _playerService = playerService;
        _notificationService = notificationService;
        _userService = userService;
    }

    [HttpGet("me")]
    [Authorize(Policy = "HasGameKey", Roles = "Player")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();
        return Ok(await _playerService.GetByUserIdAsync(userId));
    }

    [RequireGameKey]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Auth(AuthPlayerRequest request)
    {
        return Ok(await _userService.AuthenticatePlayerAsync(request));
    }

    [HttpGet("{username}/notifications")]
    public async Task<IActionResult> GetPlayerNotifications(
        string username,
        [FromQuery] PageableRequest request,
        [FromQuery] NotificationStatus status = NotificationStatus.Sent)
    {
        var notifications = await _notificationService.GetPlayerNotificationsAsync(
            username, status, request);
        return Ok(notifications);
    }

    [HttpGet("{username}/attributes/{collection}")]
    public async Task<IActionResult> GetAttributes(string username, string collection)
    {
        var attributes = await _playerService.GetAttributesAsync(collection, username);
        return Ok(attributes);
    }

    [HttpGet("{username}/attributes/{collection}/{key}")]
    public async Task<IActionResult> GetAttribute(
        string username,
        string collection,
        string key)
    {
        var attribute = await _playerService.GetAttributeAsync(username, collection, key);
        return Ok(attribute);
    }

    [HttpPut("{username}/attributes/{collection}/{key}")]
    public async Task<IActionResult> SetAttribute(
        string username,
        string collection,
        string key,
        [FromBody] AttributeRequest request)
    {
        await _playerService.SetAttributeAsync(username, collection, request);
        return Ok();
    }

    [HttpDelete("{username}/attributes/{collection}/{key}")]
    public async Task<IActionResult> DeleteAttribute(
        string username,
        string collection,
        string key)
    {
        await _playerService.RemoveAttributeAsync(username, collection, key);
        return NoContent();
    }
}