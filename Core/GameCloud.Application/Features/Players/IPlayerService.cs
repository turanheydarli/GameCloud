using GameCloud.Domain.Paging;
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
    Task<Dictionary<string, AttributeResponse>> GetAttributesAsync(string collection, string username);
    Task<AttributeResponse> GetAttributeAsync(string username, string collection, string key);
    Task SetAttributeAsync(string username, string collection, AttributeRequest request);
    Task RemoveAttributeAsync(string username, string collection, string key);
    Task ApplyAttributeUpdatesAsync(string username, IEnumerable<EntityAttributeUpdate> updates);
}