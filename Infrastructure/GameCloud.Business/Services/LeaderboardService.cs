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

    public async Task<LeaderboardRecordResponse> SubmitScoreAsync(Guid leaderboardId, LeaderboardRecordRequest request)
    {
        var leaderboard = await leaderboardRepository.GetByIdAsync(leaderboardId);
        if (leaderboard == null)
            throw new NotFoundException("Leaderboard", leaderboardId);

        var existingRecord = await leaderboardRepository.GetUserLeaderboardRecordAsync(leaderboardId, request.PlayerId);

        if (existingRecord == null)
        {
            var newRecord = new LeaderboardRecord
            {
                LeaderboardId = leaderboardId,
                PlayerId = request.PlayerId,
                Score = request.Score,
                Metadata = request.Metadata
            };
            await leaderboardRepository.CreateRecordAsync(newRecord);
            return mapper.Map<LeaderboardRecordResponse>(newRecord);
        }

        if (request.Score > existingRecord.Score)
        {
            existingRecord.Score = request.Score;
            existingRecord.Metadata = request.Metadata;
            await leaderboardRepository.UpdateRecordAsync(existingRecord);
        }

        return mapper.Map<LeaderboardRecordResponse>(existingRecord);
    }

    public async Task<List<LeaderboardRecordResponse>> GetLeaderboardRecordsAsync(Guid leaderboardId, int? limit = 100,
        int? offset = 0)
    {
        var records = await leaderboardRepository.GetLeaderboardRecordsAsync(leaderboardId, limit, offset);
        return mapper.Map<List<LeaderboardRecordResponse>>(records);
    }

    public async Task<LeaderboardRecordResponse> GetUserLeaderboardRecordAsync(Guid leaderboardId, Guid userId)
    {
        var record = await leaderboardRepository.GetUserLeaderboardRecordAsync(leaderboardId, userId);
        return record == null ? null : mapper.Map<LeaderboardRecordResponse>(record);
    }

    public async Task ResetLeaderboardAsync(Guid leaderboardId)
    {
        var leaderboard = await leaderboardRepository.GetByIdAsync(leaderboardId);
        if (leaderboard == null)
            throw new NotFoundException("Leaderboard", leaderboardId);

        await leaderboardRepository.DeleteLeaderboardRecordsAsync(leaderboardId);
    }

    public async Task<LeaderboardResponse> GetLeaderboardByNameAsync(string name)
    {
        var entity = await GetLeaderboardAsync(gameId: gameContext.GameId, leaderboardName: name);
        if (entity == null)
            throw new NotFoundException("Leaderboard", $"name: {name}");
            
        return entity;
    }
    

    public async Task<List<LeaderboardRecordResponse>> GetUserLeaderboardRecordsAsync(Guid userId, int? limit = 100)
    {
        var records = await leaderboardRepository.GetUserLeaderboardRecordsAsync(userId, limit);
        return mapper.Map<List<LeaderboardRecordResponse>>(records);
    }
}