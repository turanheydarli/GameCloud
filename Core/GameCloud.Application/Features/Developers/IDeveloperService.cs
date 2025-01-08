using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Developers;

public interface IDeveloperService
{
    Task<PageableListResponse<DeveloperResponse>> GetAllAsync(PageableRequest request);
    Task<DeveloperResponse> CreateAsync(DeveloperRequest request);
    Task<DeveloperResponse> GetByIdAsync(Guid id);
    Task<ImageResponse> SetProfilePhoto(Guid userId, ImageUploadRequest request);
    Task<DeveloperResponse> GetByUserIdAsync(Guid userId);
    Task<DeveloperResponse> UpdateAsync(Guid userId, DeveloperRequest request);
}