using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players.Responses;

namespace GameCloud.Application.Features.Players;

public interface IPlayerService
{
    Task<PageableListResponse<PlayerResponse>> GetAllAsync(PageableRequest request);
    Task<PlayerResponse> CreateAsync(PlayerRequest request);
    Task<PlayerResponse> GetByIdAsync(Guid id);
    Task<PlayerResponse> GetByUserIdAsync(Guid userId);
    

    Task<Dictionary<string, AttributeResponse?>> GetAttributesAsync(Guid userId);
    Task<AttributeResponse?> GetAttributeAsync(Guid userId, string key);
    Task SetAttributeAsync(Guid userId, AttributeRequest request);
    Task RemoveAttributeAsync(Guid playerId, string key);
    Task ApplyAttributeUpdatesAsync(string playerId, IEnumerable<EntityAttributeUpdate> updates);
}