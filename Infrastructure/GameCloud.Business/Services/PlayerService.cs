using AutoMapper;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Business.Services;

public class PlayerService(
    IPlayerRepository playerRepository,
    IMapper mapper,
    IGameContext gameContext,
    IPlayerAttributeService attributeService,
    IHttpContextAccessor httpContextAccessor)
    : IPlayerService
{
    public async Task<PlayerResponse> GetByIdAsync(Guid id)
    {
        var player = await playerRepository.GetByIdAsync(id);
        if (player == null)
            throw new NotFoundException("Player", id);

        await EnsureCanAccessPlayerData(player.Username);
        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<PlayerResponse> GetByUserIdAsync(Guid userId)
    {
        var player = await playerRepository.GetByUserIdAsync(userId);
        if (player is null)
            throw new NotFoundException("Player", userId);

        await EnsureCanAccessPlayerData(player.Username);
        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<Dictionary<string, AttributeResponse>> GetAttributesAsync(string collection, string username)
    {
        var player = await playerRepository.GetByUsernameAsync(username);
        if (player == null)
            throw new NotFoundException($"Player not found with Username: {username}");

        await EnsureCanAccessPlayerData(username);
        return await attributeService.GetCollectionAsync(player.Username, collection);
    }

    public async Task<AttributeResponse> GetAttributeAsync(string username, string collection, string key)
    {
        await EnsureCanAccessPlayerData(username);
        
        var attribute = await attributeService.GetAsync(username, collection, key);
        if (attribute == null)
            throw new NotFoundException($"Attribute {collection}/{key} not found");

        return attribute;
    }

    public async Task SetAttributeAsync(string username, string collection, AttributeRequest request)
    {
        await EnsureCanModifyPlayerData(username);

        TimeSpan? ttl = request.ExpiresIn.HasValue
            ? TimeSpan.FromSeconds(request.ExpiresIn.Value)
            : null;

        await attributeService.SetAsync(
            username,
            collection,
            request.Key,
            request.Value,
            request.ExpectedVersion,
            ttl);
    }

    public async Task RemoveAttributeAsync(string username, string collection, string key)
    {
        await EnsureCanModifyPlayerData(username);
        await attributeService.DeleteAsync(username, collection, key);
    }

    public async Task ApplyAttributeUpdatesAsync(string username, IEnumerable<EntityAttributeUpdate> updates)
    {
        await EnsureCanModifyPlayerData(username);

        var entityAttributeUpdates = updates as EntityAttributeUpdate[] ?? updates.ToArray();
        var updatesByCollection = entityAttributeUpdates.GroupBy(u => u.Collection);

        foreach (var collectionGroup in updatesByCollection)
        {
            var collection = collectionGroup.Key;
            var collectionUpdates = collectionGroup.ToDictionary(
                update => update.Key,
                update => update.Value ?? string.Empty
            );

            if (!collectionUpdates.Any() || collectionUpdates.All(x => string.IsNullOrEmpty(x.Value)))
                continue;

            await attributeService.SetBulkAsync(username, collection, collectionUpdates);
        }
    }

    public async Task<PlayerResponse> CreateAsync(PlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username must be provided.");

        var existing = await playerRepository.GetByUsernameAsync(request.Username, request.AuthProvider);
        if (existing != null)
            throw new Exception("A player with this PlayerId already exists.");

        var player = mapper.Map<Player>(request);
        player.CreatedAt = DateTime.UtcNow;
        player.UpdatedAt = DateTime.UtcNow;
        player.GameId = gameContext.GameId;

        player = await playerRepository.CreateAsync(player);
        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<PageableListResponse<PlayerResponse>> GetAllAsync(PageableRequest request)
    {
        var players = await playerRepository.GetAllAsync(request.PageIndex, request.PageSize);
        return mapper.Map<PageableListResponse<PlayerResponse>>(players);
    }

    private async Task EnsureCanAccessPlayerData(string username)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        if (currentUser == null)
            throw new UnauthorizedAccessException("No user context found");

        var currentUsername = currentUser.Identity?.Name;
        
        // Same user can always access their own data
        if (currentUsername == username) return;

        // Admin can access any data
        if (currentUser.IsInRole("Admin")) return;

        // Game specific checks can be added here
        // For example: team/guild membership checks
        
        throw new UnauthorizedAccessException(
            $"User {currentUsername} does not have permission to access data for {username}");
    }

    private async Task EnsureCanModifyPlayerData(string username)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        if (currentUser == null)
            throw new UnauthorizedAccessException("No user context found");

        var currentUsername = currentUser.Identity?.Name;

        // Only same user or admin can modify data
        if (currentUsername != username && !currentUser.IsInRole("Admin"))
        {
            throw new UnauthorizedAccessException(
                $"User {currentUsername} does not have permission to modify data for {username}");
        }
    }
}