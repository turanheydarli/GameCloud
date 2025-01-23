using System;
using System.Net;
using System.Threading.Tasks;
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

        #region Score Submission (if you want it in the same controller)

        // Example: POST /api/v1/leaderboard/scores
        // This might call something like "SubmitScoreAsync" in a dedicated service 
        // (e.g., ILeaderboardRecordService), or you can extend ILeaderboardService for it.

        // [HttpPost("scores")]
        // [ProducesResponseType(typeof(LeaderboardRecordResponse), (int)HttpStatusCode.OK)]
        // public async Task<IActionResult> SubmitScore([FromBody] LeaderboardScoreRequest request)
        // {
        //     var userId = GetUserIdFromClaims();
        //     var record = await _leaderboardService.SubmitScoreAsync(request, userId);
        //     return Ok(record);
        // }

        #endregion

        #region Archives (optional region for archive management)

        // Example: POST /api/v1/leaderboard/leaderboards/{leaderboardId}/archive
        // [HttpPost("leaderboards/{leaderboardId:guid}/archive")]
        // [ProducesResponseType(typeof(LeaderboardArchiveResponse), (int)HttpStatusCode.OK)]
        // public async Task<IActionResult> ArchiveLeaderboard([FromRoute] Guid leaderboardId)
        // {
        //     var archive = await _leaderboardService.ArchiveLeaderboardAsync(leaderboardId);
        //     return Ok(archive);
        // }

        // Example: GET /api/v1/leaderboard/leaderboards/{leaderboardId}/archives
        // [HttpGet("leaderboards/{leaderboardId:guid}/archives")]
        // [ProducesResponseType(typeof(List<LeaderboardArchiveResponse>), (int)HttpStatusCode.OK)]
        // public async Task<IActionResult> GetArchives([FromRoute] Guid leaderboardId)
        // {
        //     var archives = await _leaderboardService.GetArchivesAsync(leaderboardId);
        //     return Ok(archives);
        // }

        // Example: GET /api/v1/leaderboard/archives/{archiveId}
        // [HttpGet("archives/{archiveId:guid}")]
        // [ProducesResponseType(typeof(LeaderboardArchiveResponse), (int)HttpStatusCode.OK)]
        // [ProducesResponseType((int)HttpStatusCode.NotFound)]
        // public async Task<IActionResult> GetArchive([FromRoute] Guid archiveId)
        // {
        //     var archive = await _leaderboardService.GetArchiveAsync(archiveId);
        //     if (archive == null)
        //         return NotFound();

        //     return Ok(archive);
        // }

        #endregion
    }
}
