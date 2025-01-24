using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Sessions.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "HasGameKey")]
    public class SessionsController(
        ISessionService sessionService,
        IActionService actionService,
        IGameContext gameContext,
        IPlayerService playerService) : BaseController
    {
        [HttpPost("create")]
        [Authorize(Policy = "HasGameKey", Roles = "Player")]
        public async Task<IActionResult> CreateSession()
        {
            var userId = GetUserIdFromClaims();
            var session = await sessionService.CreateSessionAsync(userId, gameContext.GameId);

            return Ok(session);
        }

        [HttpGet("session/{sessionId}/state")]
        [Authorize(Policy = "HasGameKey", Roles = "Player")]
        public async Task<IActionResult> GetSessionState(string sessionId)
        {
            var state = await sessionService.GetSessionStateAsync(sessionId);
            throw new NotImplementedException();
        }

        [HttpPost("{sessionId}/join")]
        [Authorize(Policy = "HasGameKey", Roles = "Player")]
        public async Task<IActionResult> JoinSession(Guid sessionId, [FromBody] SessionRequest request)
        {
            if (request.PlayerId == Guid.Empty)
            {
                return BadRequest("PlayerId is required.");
            }

            await sessionService.JoinSessionAsync(sessionId, request.PlayerId);
            return Ok();
        }

        [HttpPost("{sessionId}/actions")]
        [Authorize(Policy = "HasGameKey", Roles = "Player")]
        public async Task<IActionResult> ProcessAction(Guid sessionId, [FromBody] ActionRequest request)
        {
            var userId = GetUserIdFromClaims();
            var result = await actionService.ExecuteActionAsync(sessionId, userId, request);
            return Ok(result);
        }

        [HttpGet("{sessionId}/actions")]
        [Authorize(Policy = "HasGameKey", Roles = "Player")]
        public async Task<IActionResult> GetSessionActions(Guid sessionId)
        {
            var actions = await actionService.GetActionsBySessionAsync(sessionId);
            return Ok(actions);
        }
    }
}