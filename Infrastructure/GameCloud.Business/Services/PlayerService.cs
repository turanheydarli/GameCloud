using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Application.Features.Sessions.Models;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Business.Services;

public class PlayerService(
    IPlayerRepository playerRepository,
    IMapper mapper,
    IGameContext gameContext,
    IPlayerAttributeService attributeService,
    IHttpContextAccessor httpContextAccessor,
    ITokenService tokenService,
    ISessionCache sessionCache)
    : IPlayerService
{
    public async Task<AuthenticationResponse> AuthenticateWithDeviceAsync(string deviceId, Dictionary<string, object>? metadata = null)
        {
            var player = await playerRepository.GetByDeviceIdAsync(gameContext.GameId, deviceId);
        
            if (player == null)
            {
                var guid = Guid.NewGuid();
                player = new Player
                {
                    Id = guid,
                    DeviceId = deviceId,
                    CustomId = $"device_{guid.ToString("N").Substring(0, 8)}",
                    Username = $"device_{guid.ToString("N").Substring(0, 8)}",
                    DisplayName = $"Player_{Random.Shared.Next(1000, 9999)}",
                    GameId = gameContext.GameId,
                    Metadata = JsonSerializer.SerializeToDocument(metadata ?? new Dictionary<string, object>()),
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
        
                await playerRepository.CreateAsync(player);
            }
        
            return await CreateAuthenticationResponseAsync(player, deviceId);
        }

    public async Task<AuthenticationResponse> AuthenticateWithCustomIdAsync(string customId,
        Dictionary<string, object> metadata = null, bool create = true)
    {
        var player = await playerRepository.GetByCustomIdAsync(gameContext.GameId, customId);

        if (player == null && !create)
            throw new NotFoundException($"Player with custom id {customId} not found");

        if (player == null)
        {
            player = new Player
            {
                Id = Guid.NewGuid(),
                CustomId = customId,
                Username = $"custom_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                DisplayName = $"Player_{Random.Shared.Next(1000, 9999)}",
                GameId = gameContext.GameId,
                Metadata = JsonSerializer.SerializeToDocument(metadata ?? new Dictionary<string, object>()),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await playerRepository.CreateAsync(player);
        }

        return await CreateAuthenticationResponseAsync(player, null);
    }

    public async Task<AuthenticationResponse> RefreshSessionAsync(string sessionId)
    {
        var session = await sessionCache.GetSessionAsync(sessionId);
        if (session == null)
            throw new UnauthorizedAccessException("Invalid session");

        var player = await playerRepository.GetByIdAsync(session.PlayerId);
        if (player == null)
            throw new NotFoundException("Player", session.PlayerId);

        return await CreateAuthenticationResponseAsync(player, session.DeviceId);
    }

    private async Task<AuthenticationResponse> CreateAuthenticationResponseAsync(Player player, string deviceId = null)
    {
        var sessionId = Guid.NewGuid().ToString();
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddHours(1);

        var token = await tokenService.GenerateTokenAsync(new TokenGenerationRequest
        {
            PlayerId = player.Id,
            Username = player.Username,
            SessionId = sessionId,
            DeviceId = deviceId,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt
        });

        await sessionCache.SetSessionAsync(sessionId, new SessionInfo
        {
            PlayerId = player.Id,
            SessionId = sessionId,
            DeviceId = deviceId,
            ExpiresAt = expiresAt
        });

        return new AuthenticationResponse
        {
            Player = mapper.Map<PlayerResponse>(player),
            Token = token,
            SessionId = sessionId,
            DeviceId = deviceId,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            Vars = new Dictionary<string, string>
            {
                ["game_id"] = gameContext.GameId.ToString()
            }
        };
    }

    public async Task<PlayerResponse> GetByIdAsync(Guid id)
    {
        var player = await playerRepository.GetByIdAsync(id);
        if (player == null)
            throw new NotFoundException("Player", id);

        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<PlayerResponse> GetByUserIdAsync(Guid userId)
    {
        var player = await playerRepository.GetByUserIdAsync(userId);
        if (player is null)
            throw new NotFoundException("Player", userId);

        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<AttributeCollectionResponse> GetAttributesAsync(string collection, string username)
    {
        var player = await playerRepository.GetByUsernameAsync(username);
        if (player == null)
            throw new NotFoundException($"Player not found with Username: {username}");

        return await attributeService.GetCollectionAsync(player.Username, collection);
    }

    public async Task<AttributeResponse> GetAttributeAsync(string username, string collection, string key)
    {
        var attribute = await attributeService.GetAsync(username, collection, key);
        if (attribute == null)
            throw new NotFoundException($"Attribute {collection}/{key} not found");

        return attribute;
    }

    public async Task SetAttributeAsync(string username, string collection, AttributeRequest request)
    {
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
        await attributeService.DeleteAsync(username, collection, key);
    }

    public async Task ApplyAttributeUpdatesAsync(string username, IEnumerable<EntityAttributeUpdate> updates)
    {
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
        player.DisplayName = request.Username;

        player = await playerRepository.CreateAsync(player);
        return mapper.Map<PlayerResponse>(player);
    }

    public async Task<PageableListResponse<PlayerResponse>> GetAllAsync(PageableRequest request)
    {
        var players = await playerRepository.GetAllAsync(request.PageIndex, request.PageSize);
        return mapper.Map<PageableListResponse<PlayerResponse>>(players);
    }
}