using AutoMapper;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Leaderboards;
using GameCloud.Application.Features.Leaderboards.Requests;
using GameCloud.Application.Features.Leaderboards.Responses;
using GameCloud.Domain.Dynamics;
using GameCloud.Domain.Entities.Leaderboards;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class LeaderboardService(
    IGameContext gameContext,
    ILeaderboardRepository leaderboardRepository,
    IMapper mapper)
    : ILeaderboardService
{
    public async Task<LeaderboardResponse> CreateLeaderboardAsync(LeaderboardRequest request)
    {
        var entity = new Leaderboard()
        {
            Description = request.Description,
            DisplayName = request.DisplayName,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = request.EndTimeUtc,
            GameId = gameContext.GameId,
        };

        await leaderboardRepository.CreateAsync(entity);
        return mapper.Map<LeaderboardResponse>(entity);
    }

    public async Task<LeaderboardResponse> UpdateLeaderboardAsync(Guid leaderboardId, LeaderboardRequest request)
    {
        var entity = await leaderboardRepository.GetByIdAsync(leaderboardId);
        if (entity == null)
            throw new NotFoundException("Leaderboard", leaderboardId);

        mapper.Map(request, entity);
        await leaderboardRepository.UpdateAsync(entity);
        return mapper.Map<LeaderboardResponse>(entity);
    }

    public async Task DeleteLeaderboardAsync(Guid leaderboardId)
    {
        var entity = await leaderboardRepository.GetByIdAsync(leaderboardId);
        if (entity == null)
            return;

        await leaderboardRepository.DeleteAsync(entity);
    }

    public async Task<LeaderboardResponse?> GetLeaderboardAsync(Guid? leaderboardId = null, Guid? gameId = null,
        string? leaderboardName = null)
    {
        var entity = await leaderboardRepository.GetSingleAsync(leaderboardId, gameId, leaderboardName);
        return entity == null ? null : mapper.Map<LeaderboardResponse>(entity);
    }

    public async Task<PageableListResponse<LeaderboardResponse>> GetPagedLeaderboardsAsync(DynamicRequest request)
    {
        var paged = await leaderboardRepository.GetPagedDynamicLeaderboards(request);
        return mapper.Map<PageableListResponse<LeaderboardResponse>>(paged);
    }
}