using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Application.Features.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Games;

namespace GameCloud.WebAPI.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "HasGameKey")]
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

    [HttpPost("matches/{matchId:guid}/end")]
    [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> EndMatch(
        [FromRoute] Guid matchId,
        [FromBody] JsonDocument? finalState = null)
    {
        var playerId = GetPlayerIdFromClaims();
        var result = await matchmakingService.EndMatchAsync(matchId, playerId, finalState);
        return Ok(result);
    }

    [HttpPost("matches/{matchId:guid}/leave")]
    [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> LeaveMatch([FromRoute] Guid matchId)
    {
        var playerId = GetPlayerIdFromClaims();
        var result = await matchmakingService.LeaveMatchAsync(matchId, playerId);
        return Ok(result);
    }

    [HttpGet("queues")]
    [ProducesResponseType(typeof(PageableListResponse<MatchmakingResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueues(
        [FromQuery] string? search,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10)
    {
        var request = new PageableRequest { PageIndex = pageIndex, PageSize = pageSize };
        var response = await matchmakingService.GetQueuesAsync(gameContext.GameId, search, request);
        return Ok(response);
    }

    [HttpPut("queues/{queueId:guid}")]
    [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateQueue(
        [FromRoute] Guid queueId,
        [FromBody] MatchQueueRequest request)
    {
        var response = await matchmakingService.UpdateQueueAsync(queueId, request);
        return Ok(response);
    }

    [HttpDelete("queues/{queueId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteQueue([FromRoute] Guid queueId)
    {
        await matchmakingService.DeleteQueueAsync(queueId);
        return Ok();
    }

    [HttpPost("queues/{queueId:guid}/toggle")]
    [ProducesResponseType(typeof(QueueToggleResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ToggleQueue(
        [FromRoute] Guid queueId,
        [FromBody] ToggleQueueRequest request)
    {
        var response = await matchmakingService.ToggleQueueAsync(queueId, request.IsEnabled);
        return Ok(response);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(MatchmakingStatsResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetStats(
        [FromQuery] List<Guid>? queueIds,
        [FromQuery] string timeRange = "24h")
    {
        var response = await matchmakingService.GetMatchmakingStatsAsync(gameContext.GameId, queueIds, timeRange);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/activity")]
    [ProducesResponseType(typeof(QueueActivityResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueActivity(
        [FromRoute] Guid queueId,
        [FromQuery] string timeRange = "24h")
    {
        var response = await matchmakingService.GetQueueActivityAsync(queueId, timeRange);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/matches")]
    [ProducesResponseType(typeof(PageableListResponse<MatchResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueMatches(
        [FromRoute] Guid queueId,
        [FromQuery] string? status,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10)
    {
        var request = new PageableRequest { PageIndex = pageIndex, PageSize = pageSize };
        var response = await matchmakingService.GetQueueMatchesAsync(queueId, status, request);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/tickets")]
    [ProducesResponseType(typeof(PageableListResponse<MatchTicketResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueTickets(
        [FromRoute] Guid queueId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10)
    {
        var request = new PageableRequest { PageIndex = pageIndex, PageSize = pageSize };
        var response = await matchmakingService.GetQueueTicketsAsync(queueId, request);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/functions")]
    [ProducesResponseType(typeof(QueueFunctionsResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueFunctions([FromRoute] Guid queueId)
    {
        var response = await matchmakingService.GetQueueFunctionsAsync(queueId);
        return Ok(response);
    }

    [HttpPut("queues/{queueId:guid}/functions")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateQueueFunctions(
        [FromRoute] Guid queueId,
        [FromBody] UpdateQueueFunctionsRequest request)
    {
        await matchmakingService.UpdateQueueFunctionsAsync(queueId, request);
        return Ok();
    }

    [HttpPost("queues/{queueId:guid}/test")]
    [ProducesResponseType(typeof(QueueTestResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> TestQueue(
        [FromRoute] Guid queueId,
        [FromBody] QueueTestRequest request)
    {
        var response = await matchmakingService.TestQueueAsync(queueId, request);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/dashboard")]
    [ProducesResponseType(typeof(QueueDashboardResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueDashboard([FromRoute] Guid queueId)
    {
        var response = await matchmakingService.GetQueueDashboardAsync(queueId);
        return Ok(response);
    }

    [HttpGet("queues/{queueId:guid}/rules")]
    [ProducesResponseType(typeof(JsonDocument), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetQueueRules([FromRoute] Guid queueId)
    {
        var response = await matchmakingService.GetQueueRulesAsync(queueId);
        return Ok(response);
    }

    [HttpPut("queues/{queueId:guid}/rules")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateQueueRules(
        [FromRoute] Guid queueId,
        [FromBody] JsonDocument rules)
    {
        await matchmakingService.UpdateQueueRulesAsync(queueId, rules);
        return Ok();
    }
}

public record ToggleQueueRequest(bool IsEnabled);

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