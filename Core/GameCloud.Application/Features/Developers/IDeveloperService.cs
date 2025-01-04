using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;

namespace GameCloud.Application.Features.Developers;

public interface IDeveloperService
{
    Task<PageableListResponse<DeveloperResponse>> GetAllAsync(PageableRequest request);
    Task<DeveloperResponse> CreateAsync(DeveloperRequest request);
    Task<DeveloperResponse> GetByIdAsync(Guid id);
    Task<DeveloperResponse> GetByUserIdAsync(Guid userId);
    Task<DeveloperResponse> UpdateAsync(Guid userId, DeveloperRequest request);
}