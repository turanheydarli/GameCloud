using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GameCloud.Business.Services;

public class PlayerAttributeService(
    IPlayerAttributeRepository repository,
    IMapper mapper,
    ILogger<PlayerAttributeService> logger,
    IPermissionValidator permissionValidator)
    : IPlayerAttributeService
{
    private const int MaxValueLength = 1024 * 1024;
    private readonly IMapper _mapper = mapper;

    public async Task<Dictionary<string, AttributeResponse>> GetCollectionAsync(
        string username,
        string collection)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection is required", nameof(collection));

        var cacheKey = $"attr:collection:{username}:{collection}";
        // var cached = await _cache.GetAsync<Dictionary<string, AttributeResponse>>(cacheKey);
        // if (cached != null)
        //     return cached;

        var attributes = await repository.GetCollectionAsync(username, collection);
        
        var result = attributes.ToDictionary(
            attr => attr.Key,
            attr => new AttributeResponse(
                attr.Username,
                attr.Collection,
                attr.Key,
                attr.Value,
                attr.Version,
                attr.PermissionRead.Deserialize<Dictionary<string, object>>(),
                attr.PermissionWrite.Deserialize<Dictionary<string, object>>()
            )
        );

        // await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<AttributeResponse?> GetAsync(
        string username,
        string collection,
        string key)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection is required", nameof(collection));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key is required", nameof(key));

        var cacheKey = $"attr:{username}:{collection}:{key}";
        // var cached = await _cache.GetAsync<AttributeResponse>(cacheKey);
        // if (cached != null)
        //     return cached;

        var attribute = await repository.GetAsync(username, collection, key);
        if (attribute == null)
            return null;

        await ValidatePermissions(username, attribute, PermissionType.Read);

        var response = new AttributeResponse(
            attribute.Username,
            attribute.Collection,
            attribute.Key,
            attribute.Value,
            attribute.Version,
            attribute.PermissionRead.Deserialize<Dictionary<string, object>>(),
            attribute.PermissionWrite.Deserialize<Dictionary<string, object>>()
        );

        // await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
        return response;
    }

    public async Task SetAsync(
        string username,
        string collection,
        string key,
        string value,
        string? expectedVersion = null,
        TimeSpan? ttl = null)
    {
        // Input validation
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection is required", nameof(collection));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key is required", nameof(key));
        if (value.Length > MaxValueLength)
            throw new ArgumentException($"Value exceeds maximum length of {MaxValueLength}", nameof(value));

        logger.LogInformation(
            "Setting attribute {Collection}/{Key} for user {Username}",
            collection, key, username);


        var attribute = await repository.GetAsync(username, collection, key);
        var oldValue = attribute?.Value;

        if (attribute != null)
        {
            await ValidatePermissions(username, attribute, PermissionType.Write);

            if (expectedVersion != null && attribute.Version != expectedVersion)
                throw new ConcurrencyException("Attribute was modified by another process");

            attribute.Value = value;
            attribute.Version = DateTime.UtcNow.Ticks.ToString("x");
            attribute.UpdatedAt = DateTime.UtcNow;
            if (ttl.HasValue)
                attribute.ExpiresAt = DateTime.UtcNow.Add(ttl.Value);

            await repository.UpdateAsync(attribute);
        }
        else
        {
            attribute = new PlayerAttribute
            {
                Username = username,
                Collection = collection,
                Key = key,
                Value = value,
                Version = DateTime.UtcNow.Ticks.ToString("x"),
                PermissionRead = JsonDocument.Parse("{}"),
                PermissionWrite = JsonDocument.Parse("{}"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null
            };

            await repository.CreateAsync(attribute);
        }


        // Invalidate cache
        var cacheKey = $"attr:{username}:{collection}:{key}";
        // await _cache.RemoveAsync(cacheKey);
        // await _cache.RemoveAsync($"attr:collection:{username}:{collection}");
    }

    public async Task SetBulkAsync(
        string username,
        string collection,
        Dictionary<string, string> attributes)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection is required", nameof(collection));
        if (!attributes.Any())
            throw new ArgumentException("At least one attribute is required", nameof(attributes));

        foreach (var (key, value) in attributes)
        {
            await SetAsync(username, collection, key, value);
        }
    }

    public async Task DeleteAsync(
        string username,
        string collection,
        string key)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrEmpty(collection))
            throw new ArgumentException("Collection is required", nameof(collection));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key is required", nameof(key));

        var attribute = await repository.GetAsync(username, collection, key);
        if (attribute == null)
            return;

        await ValidatePermissions(username, attribute, PermissionType.Write);


        await repository.DeleteAsync(username, collection, key);

        var cacheKey = $"attr:{username}:{collection}:{key}";
        // await cache.RemoveAsync(cacheKey);
        // await cache.RemoveAsync($"attr:collection:{username}:{collection}");
    }

    private async Task ValidatePermissions(
        string username,
        PlayerAttribute attribute,
        PermissionType type)
    {
        var permissions = type == PermissionType.Read
            ? attribute.PermissionRead
            : attribute.PermissionWrite;

        if (!await permissionValidator.HasPermission(username, permissions))
            throw new UnauthorizedAccessException($"User {username} does not have {type} permission");
    }
}