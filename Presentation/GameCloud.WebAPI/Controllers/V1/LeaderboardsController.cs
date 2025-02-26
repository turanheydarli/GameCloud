using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameCloud.Application.Features.Leaderboards;
using GameCloud.Application.Features.Leaderboards.Requests;
using GameCloud.Application.Features.Leaderboards.Responses;
using GameCloud.Application.Common.Responses;
using GameCloud.Domain.Dynamics;

namespace GameCloud.WebAPI.Controllers.V1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Policy = "HasGameKey")]
    public class LeaderboardsController(ILeaderboardService leaderboardService) : BaseController
    {
        #region Leaderboard Management

        [HttpPost]
        [ProducesResponseType(typeof(LeaderboardResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateLeaderboard([FromBody] LeaderboardRequest request)
        {
            var created = await leaderboardService.CreateLeaderboardAsync(request);
            return Ok(created);
        }

        [HttpGet("{leaderboardId:guid}")]
        [ProducesResponseType(typeof(LeaderboardResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLeaderboard([FromRoute] Guid leaderboardId)
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync(leaderboardId);
            if (leaderboard == null)
                return NotFound();

            return Ok(leaderboard);
        }

        [HttpPut("{leaderboardId:guid}")]
        [ProducesResponseType(typeof(LeaderboardResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateLeaderboard(
            [FromRoute] Guid leaderboardId,
            [FromBody] LeaderboardRequest request)
        {
            var updated = await leaderboardService.UpdateLeaderboardAsync(leaderboardId, request);
            return Ok(updated);
        }

        [HttpDelete("{leaderboardId:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteLeaderboard([FromRoute] Guid leaderboardId)
        {
            await leaderboardService.DeleteLeaderboardAsync(leaderboardId);
            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(typeof(PageableListResponse<LeaderboardResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLeaderboards([FromQuery] DynamicRequest request)
        {
            var results = await leaderboardService.GetPagedLeaderboardsAsync(request);
            return Ok(results);
        }

        #endregion

        #region Score Management

        [HttpPost("scores")]
        [ProducesResponseType(typeof(LeaderboardRecordResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SubmitScore([FromBody] LeaderboardRecordRequest request)
        {
            var record = await leaderboardService.SubmitScoreAsync(request.LeaderboardId, request);
            return Ok(record);
        }

        [HttpGet("{leaderboardId:guid}/records")]
        [ProducesResponseType(typeof(List<LeaderboardRecordResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLeaderboardRecords(
            [FromRoute] Guid leaderboardId,
            [FromQuery] int? limit = 100,
            [FromQuery] int? offset = 0)
        {
            var records = await leaderboardService.GetLeaderboardRecordsAsync(leaderboardId, limit, offset);
            return Ok(records);
        }

        [HttpGet("{leaderboardId:guid}/records/{userId:guid}")]
        [ProducesResponseType(typeof(LeaderboardRecordResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserLeaderboardRecord(
            [FromRoute] Guid leaderboardId,
            [FromRoute] Guid userId)
        {
            var record = await leaderboardService.GetUserLeaderboardRecordAsync(leaderboardId, userId);

            return Ok(record);
        }

        [HttpGet("records/{userId:guid}")]
        [ProducesResponseType(typeof(List<LeaderboardRecordResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserLeaderboardRecords(
            [FromRoute] Guid userId,
            [FromQuery] int? limit = 100)
        {
            var records = await leaderboardService.GetUserLeaderboardRecordsAsync(userId, limit);
            return Ok(records);
        }

        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(LeaderboardResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLeaderboardByName([FromRoute] string name)
        {
            var leaderboard = await leaderboardService.GetLeaderboardByNameAsync(name);

            return Ok(leaderboard);
        }

        [HttpPost("{leaderboardId:guid}/reset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ResetLeaderboard([FromRoute] Guid leaderboardId)
        {
            await leaderboardService.ResetLeaderboardAsync(leaderboardId);
            return NoContent();
        }

        #endregion
    }
}