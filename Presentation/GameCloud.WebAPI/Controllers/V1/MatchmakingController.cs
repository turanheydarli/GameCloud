using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Application.Features.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.WebAPI.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "HasGameKey")]
    public class MatchmakingController(
        IMatchmakingService matchmakingService,
        IPlayerService playerService)
        : BaseController
    {

        #region Queue Management

        [HttpPost("queues")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateQueue([FromBody] MatchQueueRequest request)
        {
            var response = await matchmakingService.CreateQueueAsync(request);
            return Ok(response);
        }

        [HttpGet("queues/{queueId:guid}")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetQueue([FromRoute] Guid queueId)
        {
            var queue = await matchmakingService.GetQueueAsync(queueId: queueId);
            return Ok(queue);
        }

        [HttpPut("queues/{queueId:guid}")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateQueue([FromRoute] Guid queueId, [FromBody] MatchQueueRequest request)
        {
            var updated = await matchmakingService.UpdateQueueAsync(queueId, request);
            return Ok(updated);
        }

        [HttpDelete("queues/{queueId:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteQueue([FromRoute] Guid queueId)
        {
            await matchmakingService.DeleteQueueAsync(queueId);
            return NoContent();
        }

        #endregion

        #region Single-Call Find/Join Match (Optional)

        [HttpPost("matches/find")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FindOrCreateMatch([FromBody] FindMatchRequest request)
        {
            var userId = GetUserIdFromClaims();
            var player = await playerService.GetByUserIdAsync(userId);
            var match = await matchmakingService.FindOrCreateMatchAsync(request, player.Id);
            return Ok(match);
        }

        #endregion

        #region Ticket Lifecycle

        [HttpPost("tickets")]
        [ProducesResponseType(typeof(MatchTicketResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EnqueuePlayer([FromBody] EnqueuePlayerRequest request)
        {
            // EnqueuePlayerRequest is a custom DTO you might create 
            // that includes { GameId, PlayerId, QueueName, CustomProperties } 
            // or reuse direct parameters.

            var ticket = await matchmakingService.EnqueuePlayerAsync(
                request.GameId,
                request.PlayerId,
                request.QueueName,
                request.CustomProperties);

            return Ok(ticket);
        }

        // DELETE /api/v1/matchmaking/tickets/{ticketId}
        [HttpDelete("tickets/{ticketId:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CancelTicket([FromRoute] Guid ticketId)
        {
            await matchmakingService.CancelTicketAsync(ticketId);
            return NoContent();
        }

        // GET /api/v1/matchmaking/tickets/{ticketId}
        [HttpGet("tickets/{ticketId:guid}")]
        [ProducesResponseType(typeof(MatchTicketResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicket([FromRoute] Guid ticketId)
        {
            var ticket = await matchmakingService.GetTicketAsync(ticketId);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        #endregion

        #region Accept/Decline (Real-Time)

        // PUT /api/v1/matchmaking/tickets/{ticketId}/accept
        [HttpPut("tickets/{ticketId:guid}/accept")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> AcceptMatch([FromRoute] Guid ticketId)
        {
            await matchmakingService.AcceptMatchAsync(ticketId);
            return NoContent();
        }

        // PUT /api/v1/matchmaking/tickets/{ticketId}/decline
        [HttpPut("tickets/{ticketId:guid}/decline")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeclineMatch([FromRoute] Guid ticketId)
        {
            await matchmakingService.DeclineMatchAsync(ticketId);
            return NoContent();
        }

        #endregion

        #region Match Lifecycle

        // GET /api/v1/matchmaking/matches/{matchId}
        [HttpGet("matches/{matchId:guid}")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetMatch([FromRoute] Guid matchId)
        {
            var match = await matchmakingService.GetMatchAsync(matchId);
            if (match == null)
                return NotFound();

            return Ok(match);
        }

        [HttpPut("matches/{matchId:guid}/state")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMatchState(
            [FromRoute] Guid matchId,
            [FromBody] MatchState request)
        {
            // e.g. { newState = "InProgress" }
            var updated = await matchmakingService.UpdateMatchStateAsync(matchId, request);
            return Ok(updated);
        }

        [HttpDelete("matches/{matchId:guid}")]
        public async Task<IActionResult> CancelMatch([FromRoute] Guid matchId)
        {
            await matchmakingService.CancelMatchAsync(matchId);
            return NoContent();
        }

        [HttpPost("matches/process")]
        [ProducesResponseType(typeof(List<MatchResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ProcessMatchmaking([FromQuery] Guid? queueId = null)
        {
            var matches = await matchmakingService.ProcessMatchmakingAsync(queueId);
            return Ok(matches);
        }

        [HttpGet("tickets/{ticketId:guid}/match")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CheckMatchStatus([FromRoute] Guid ticketId)
        {
            var match = await matchmakingService.CheckMatchStatusAsync(ticketId);
            return Ok(match); 
        }

        #endregion
    }

    public record UpdateMatchStateRequest(MatchState NewState);

    public record EnqueuePlayerRequest(
        Guid GameId,
        Guid PlayerId,
        string QueueName,
        JsonDocument? CustomProperties
    );
}