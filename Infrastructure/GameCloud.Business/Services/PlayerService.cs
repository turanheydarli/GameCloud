using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services
{
    public class PlayerService(IPlayerRepository playerRepository, IMapper mapper, IGameContext gameContext)
        : IPlayerService
    {
        public async Task<PlayerResponse> GetByIdAsync(Guid id)
        {
            var player = await playerRepository.GetByIdAsync(id);
            if (player == null)
            {
                throw new NotFoundException("Player", id);
            }

            return mapper.Map<PlayerResponse>(player);
        }

        public async Task<PlayerResponse> GetByUserIdAsync(Guid userId)
        {
            var player = await playerRepository.GetByUserIdAsync(userId);
            if (player is null)
            {
                throw new NotFoundException("Player", userId);
            }

            return mapper.Map<PlayerResponse>(player);
        }

        public async Task ApplyAttributeUpdatesAsync(string playerId, IEnumerable<EntityAttributeUpdate> updates)
        {
            var player = await playerRepository.GetByPlayerIdAsync(playerId);
            if (player == null)
            {
                throw new NotFoundException("Player", playerId);
            }

            var existingAttributes = player.Attributes.Deserialize<Dictionary<string, object>>()
                                     ?? new Dictionary<string, object>();

            foreach (var update in updates)
            {
                var key = update.Key;
                var newValue = update.Value;

                if (newValue == null)
                {
                    existingAttributes.Remove(key);
                }
                else
                {
                    if (existingAttributes.TryGetValue(key, out var currentValue))
                    {
                        existingAttributes[key] = MergeObjects(currentValue, newValue);
                    }
                    else
                    {
                        existingAttributes[key] = newValue;
                    }
                }
            }

            player.Attributes = JsonDocument.Parse(JsonSerializer.Serialize(existingAttributes));
            player.UpdatedAt = DateTime.UtcNow;

            await playerRepository.UpdateAsync(player);
        }


        public async Task<Dictionary<string, AttributeResponse>> GetAttributesAsync(Guid userId)
        {
            var attributes = await GetPlayerAttributesAsync(userId);
            return attributes.ToDictionary(
                kvp => kvp.Key,
                kvp => new AttributeResponse(kvp.Key, kvp.Value, kvp.Value?.GetType().Name ?? "null")
            );
        }

        public async Task<AttributeResponse?> GetAttributeAsync(Guid userId, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            var attributes = await GetPlayerAttributesAsync(userId);
            if (!attributes.TryGetValue(key, out var attribute))
            {
                throw new NotFoundException("Attribute", key);
            }

            return new AttributeResponse(key, attribute, attribute.GetType().Name);
        }

        public async Task SetAttributeAsync(Guid userId, AttributeRequest request)
        {
            var player = await playerRepository.GetByUserIdAsync(userId);
            if (player == null)
            {
                throw new NotFoundException("User", userId);
            }

            var attributes = player.Attributes.Deserialize<Dictionary<string, object>>()
                             ?? new Dictionary<string, object>();

            attributes[request.Key] = request.Value;
            player.Attributes = JsonDocument.Parse(JsonSerializer.Serialize(attributes));
            player.UpdatedAt = DateTime.UtcNow;

            await playerRepository.UpdateAsync(player);
        }

        public async Task RemoveAttributeAsync(Guid playerId, string key)
        {
            var player = await playerRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                throw new NotFoundException("Player", playerId);
            }

            var attributes = player.Attributes.Deserialize<Dictionary<string, object>>()
                             ?? new Dictionary<string, object>();

            if (attributes.Remove(key))
            {
                player.Attributes = JsonDocument.Parse(JsonSerializer.Serialize(attributes));
                player.UpdatedAt = DateTime.UtcNow;
                await playerRepository.UpdateAsync(player);
            }
            else
            {
                throw new NotFoundException("Attribute", key);
            }
        }

        public async Task<PlayerResponse> CreateAsync(PlayerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerId))
            {
                throw new ArgumentException("PlayerId must be provided.");
            }

            var existing = await playerRepository.GetByPlayerIdAsync(request.PlayerId, request.AuthProvider);
            if (existing != null)
            {
                //TODO: Convert to a more specific ConflictException if desired.
                throw new Exception("A player with this PlayerId already exists.");
            }

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


        private static object MergeObjects(object? targetValue, object? sourceValue)
        {
            if (sourceValue == null) return targetValue!;
            if (targetValue == null) return sourceValue;

            // 1) Both are dictionaries => deep merge
            if (targetValue is Dictionary<string, object> targetDict &&
                sourceValue is Dictionary<string, object> sourceDict)
            {
                MergeDictionaries(targetDict, sourceDict);
                return targetDict;
            }
            // 2) Both are lists => append
            else if (targetValue is List<object> targetList &&
                     sourceValue is List<object> sourceList)
            {
                targetList.AddRange(sourceList);
                return targetList;
            }
            // 3) Otherwise => override
            else
            {
                return sourceValue;
            }
        }

        private static void MergeDictionaries(
            Dictionary<string, object> target,
            Dictionary<string, object> source)
        {
            foreach (var (key, value) in source)
            {
                if (target.ContainsKey(key))
                {
                    target[key] = MergeObjects(target[key], value);
                }
                else
                {
                    target[key] = value;
                }
            }
        }

        private async Task<Dictionary<string, object>> GetPlayerAttributesAsync(Guid userId)
        {
            var player = await playerRepository.GetByUserIdAsync(userId);
            if (player == null)
            {
                throw new NotFoundException("Player", userId);
            }

            return player.Attributes.Deserialize<Dictionary<string, object>>()
                   ?? new Dictionary<string, object>();
        }
    }
}