using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
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
        IActionService actionService) : BaseController
    {
        [HttpPost("{sessionId}/join")]
        public async Task<IActionResult> JoinSession(Guid sessionId, [FromBody] JoinSessionRequest request)
        {
            if (request.PlayerId == Guid.Empty)
            {
                return BadRequest("PlayerId is required.");
            }

            await sessionService.JoinSessionAsync(sessionId, request.PlayerId);
            return Ok();
        }

        [HttpPost("{sessionId}/actions")]
        public async Task<IActionResult> ProcessAction(Guid sessionId, [FromBody] ActionRequest request)
        {
            var result = await actionService.ExecuteActionAsync(sessionId, request);
            return Ok(result);
        }

        [HttpGet("{sessionId}/actions")]
        public async Task<IActionResult> GetSessionActions(Guid sessionId)
        {
            var actions = await actionService.GetActionsBySessionAsync(sessionId);
            return Ok(actions);
        }
    }
}