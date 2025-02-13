using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Application.Features.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using GameCloud.Application.Features.Games;

namespace GameCloud.WebAPI.Controllers.V1;

[ApiController]
[Authorize(Policy = "HasGameKey")]
[Route("api/v1/[controller]")]
public class MatchmakingController(
    IMatchmakingService matchmakingService,
    IPlayerService playerService,
    IGameContext gameContext)
    : BaseController
{
    [HttpPost("queues")]
    [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateQueue([FromBody] MatchQueueRequest request)
    {
        var response = await matchmakingService.CreateQueueAsync(request);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}")]
    [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueue(
        [FromRoute] Guid queueId,
        [FromQuery] Guid? gameId = null,
        [FromQuery] string? queueName = null)
    {
        var queue = await matchmakingService.GetQueueAsync(queueId, gameId, queueName);
        if (queue == null) return NotFound();
        return Ok(queue);
    }

    [HttpPost("tickets")]
    [ProducesResponseType(typeof(MatchTicketResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var playerId = GetPlayerIdFromClaims();

        var ticket = await matchmakingService.CreateTicketAsync(
            gameContext.GameId,
            playerId,
            request.QueueName,
            request.Properties
        );
        return Ok(ticket);
    }

    [HttpGet("tickets/{ticketId:guid}")]
    [ProducesResponseType(typeof(MatchTicketResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetTicket(Guid ticketId)
    {
        var playerId = GetPlayerIdFromClaims();

        var ticket = await matchmakingService.GetTicket(
            gameContext.GameId,
            playerId,
            ticketId
        );
        return Ok(ticket);
    }

    [HttpGet("matches/{matchId:guid}")]
    [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMatch([FromRoute] Guid matchId)
    {
        var match = await matchmakingService.GetMatchAsync(matchId);
        return Ok(match);
    }

    [HttpGet("matches/{matchId:guid}/state")]
    [ProducesResponseType(typeof(JsonDocument), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMatchState([FromRoute] Guid matchId)
    {
        var state = await matchmakingService.GetMatchStateAsync(matchId);
        return Ok(state);
    }

    [HttpPost("matches/{matchId:guid}/actions")]
    [ProducesResponseType(typeof(MatchActionResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitAction(
        [FromRoute] Guid matchId,
        [FromBody] MatchActionRequest request)
    {
        var playerId = GetPlayerIdFromClaims();
        
        var action = await matchmakingService.SubmitActionAsync(matchId, playerId, request);
        return Ok(action);
    }

    [HttpGet("matches/{matchId:guid}/actions")]
    [ProducesResponseType(typeof(List<MatchActionResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMatchActions(
        [FromRoute] Guid matchId,
        [FromQuery] DateTime? since = null,
        [FromQuery] int? limit = null)
    {
        var actions = await matchmakingService.GetMatchActionsAsync(matchId, since, limit);
        return Ok(actions);
    }

    [HttpPost("process")]
    [ProducesResponseType(typeof(List<MatchResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ProcessMatchmaking([FromQuery] Guid? queueId = null)
    {
        var matches = await matchmakingService.ProcessMatchmakingAsync(queueId);
        return Ok(matches);
    }

    [HttpPost("matches/{matchId:guid}/ready")]
    [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> MarkPlayerReady([FromRoute] Guid matchId)
    {
        var playerId = GetPlayerIdFromClaims();
        var match = await matchmakingService.MarkPlayerReadyAsync(matchId, playerId);
        return Ok(match);
    }

    public record UpdatePresenceRequest(
        string SessionId,
        PresenceStatus Status,
        JsonDocument? Meta = null
    );

    [HttpPost("matches/{matchId:guid}/presence")]
    [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdatePresence(
        [FromRoute] Guid matchId,
        [FromBody] UpdatePresenceRequest request)
    {
        var playerId = GetPlayerIdFromClaims();

        var result = await matchmakingService.UpdatePresenceAsync(
            matchId,
            playerId,
            request.SessionId,
            request.Status,
            request.Meta ?? JsonDocument.Parse("{}")
        );

        return Ok(result);
    }
}

public record CreateTicketRequest(
    string QueueName,
    JsonDocument? Properties
);

public record UpdateMatchStateRequest(
    JsonDocument PlayerStates,
    JsonDocument MatchState,
    Guid? NextPlayerId,
    DateTime? NextDeadline
);