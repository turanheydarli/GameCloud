using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Matchmakers;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Games;
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
    public class MatchmakingController : BaseController
    {
        private readonly ISessionService _sessionService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameContext _gameContext;
        private readonly IPlayerService _playerService;

        public MatchmakingController(
            ISessionService sessionService,
            IMatchmakingService matchmakingService,
            IGameContext gameContext,
            IPlayerService playerService)
        {
            _sessionService = sessionService;
            _matchmakingService = matchmakingService;
            _gameContext = gameContext;
            _playerService = playerService;
        }

        #region Queue Management

        [HttpPost("queues")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateQueue([FromBody] MatchQueueRequest request)
        {
            var response = await _matchmakingService.CreateQueueAsync(request);
            return Ok(response);
        }

        [HttpGet("queues/{queueId:guid}")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetQueue([FromRoute] Guid queueId)
        {
            var queue = await _matchmakingService.GetQueueAsync(queueId: queueId);
            return Ok(queue);
        }

        [HttpPut("queues/{queueId:guid}")]
        [ProducesResponseType(typeof(MatchmakingResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateQueue([FromRoute] Guid queueId, [FromBody] MatchQueueRequest request)
        {
            var updated = await _matchmakingService.UpdateQueueAsync(queueId, request);
            return Ok(updated);
        }

        [HttpDelete("queues/{queueId:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteQueue([FromRoute] Guid queueId)
        {
            await _matchmakingService.DeleteQueueAsync(queueId);
            return NoContent();
        }

        #endregion

        #region Single-Call Find/Join Match (Optional)

        [HttpPost("matches/find")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FindOrCreateMatch([FromBody] FindMatchRequest request)
        {
            var userId = GetUserIdFromClaims();
            var player = await _playerService.GetByUserIdAsync(userId);
            var match = await _matchmakingService.FindOrCreateMatchAsync(request, player.Id);
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

            var ticket = await _matchmakingService.EnqueuePlayerAsync(
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
            await _matchmakingService.CancelTicketAsync(ticketId);
            return NoContent();
        }

        // GET /api/v1/matchmaking/tickets/{ticketId}
        [HttpGet("tickets/{ticketId:guid}")]
        [ProducesResponseType(typeof(MatchTicketResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicket([FromRoute] Guid ticketId)
        {
            var ticket = await _matchmakingService.GetTicketAsync(ticketId);
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
            await _matchmakingService.AcceptMatchAsync(ticketId);
            return NoContent();
        }

        // PUT /api/v1/matchmaking/tickets/{ticketId}/decline
        [HttpPut("tickets/{ticketId:guid}/decline")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeclineMatch([FromRoute] Guid ticketId)
        {
            await _matchmakingService.DeclineMatchAsync(ticketId);
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
            var match = await _matchmakingService.GetMatchAsync(matchId);
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
            var updated = await _matchmakingService.UpdateMatchStateAsync(matchId, request);
            return Ok(updated);
        }

        // DELETE /api/v1/matchmaking/matches/{matchId}
        [HttpDelete("matches/{matchId:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CancelMatch([FromRoute] Guid matchId)
        {
            await _matchmakingService.CancelMatchAsync(matchId);
            return NoContent();
        }

        // POST /api/v1/matchmaking/matches/process
        [HttpPost("matches/process")]
        [ProducesResponseType(typeof(List<MatchResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ProcessMatchmaking([FromQuery] Guid? queueId = null)
        {
            var matches = await _matchmakingService.ProcessMatchmakingAsync(queueId);
            return Ok(matches);
        }

        // GET /api/v1/matchmaking/tickets/{ticketId}/match
        [HttpGet("tickets/{ticketId:guid}/match")]
        [ProducesResponseType(typeof(MatchResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CheckMatchStatus([FromRoute] Guid ticketId)
        {
            var match = await _matchmakingService.CheckMatchStatusAsync(ticketId);
            return Ok(match); // could be null; handle accordingly
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