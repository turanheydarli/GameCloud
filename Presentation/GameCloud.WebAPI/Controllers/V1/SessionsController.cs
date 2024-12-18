using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Sessions;
using GameCloud.Application.Features.Sessions.Requests;
using GameCloud.WebAPI.Filters.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1
{
    [RequireGameKey]
    [Route("api/v1/[controller]")]
    public class SessionsController : BaseController
    {
        private readonly ISessionService _sessionService;
        private readonly IActionService _actionService;

        public SessionsController(
            ISessionService sessionService,
            IActionService actionService)
        {
            _sessionService = sessionService;
            _actionService = actionService;
        }

        [HttpPost("{sessionId}/join")]
        public async Task<IActionResult> JoinSession(Guid sessionId, [FromBody] JoinSessionRequest request)
        {
            if (request.PlayerId == Guid.Empty)
            {
                return BadRequest("PlayerId is required.");
            }

            await _sessionService.JoinSessionAsync(sessionId, request.PlayerId);
            return Ok();
        }

        [HttpPost("{sessionId}/actions")]
        public async Task<IActionResult> ProcessAction(Guid sessionId, [FromBody] ActionRequest request)
        {
            var result = await _actionService.ExecuteActionAsync(sessionId, request);
            return Success(result);
        }

        [HttpGet("{sessionId}/actions")]
        public async Task<IActionResult> GetSessionActions(Guid sessionId)
        {
            var actions = await _actionService.GetActionsBySessionAsync(sessionId);
            return Success(actions);
        }
    }
}