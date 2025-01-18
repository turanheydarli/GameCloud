using AutoMapper;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Business.Services;

public class DeveloperService(IDeveloperRepository developerRepository,
    IImageService imageService,IMapper mapper) : IDeveloperService
{
    public async Task<PageableListResponse<DeveloperResponse>> GetAllAsync(PageableRequest request)
    {
        var developers = await developerRepository.GetAllAsync(request.PageIndex, request.PageSize);

        return mapper.Map<PageableListResponse<DeveloperResponse>>(developers);
    }

    public async Task<DeveloperResponse> CreateAsync(DeveloperRequest request)
    {
        var developer = mapper.Map<Developer>(request);

        developer = await developerRepository.CreateAsync(developer);

        return mapper.Map<DeveloperResponse>(developer);
    }

    public async Task<DeveloperResponse> GetByIdAsync(Guid id)
    {
        var developer = await developerRepository.GetByIdAsync(id);

        if (developer is null)
        {
            throw new NotFoundException("Developer", id);
        }

        return mapper.Map<DeveloperResponse>(developer);
    }

    public async Task<ImageResponse> SetProfilePhoto(Guid userId, ImageUploadRequest request)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);

        if (developer is null)
        {
            throw new NotFoundException("User", userId);
        }
        
        if (developer.ProfilePhotoId.HasValue)
        {
            await imageService.DeleteAsync(developer.ProfilePhotoId.Value);
        }

        request.Type = ImageType.DeveloperProfile;
        var imageResponse = await imageService.UploadAsync(request);

        developer.ProfilePhotoId = imageResponse.Id;
        developer.UpdatedAt = DateTime.UtcNow;

        await developerRepository.UpdateAsync(developer);

        return imageResponse;
    }

    public async Task<DeveloperResponse> GetByUserIdAsync(Guid userId)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);

        if (developer is null)
        {
            throw new NotFoundException("User", userId);
        }

        return mapper.Map<DeveloperResponse>(developer);
    }

    public async Task<DeveloperResponse> UpdateAsync(Guid userId, DeveloperRequest request)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);

        if (developer is null)
        {
            throw new NotFoundException("User", userId);
        }

        developer.Email = request.Email;
        developer.Name = request.Name;
        developer.UpdatedAt = DateTime.UtcNow;

        await developerRepository.UpdateAsync(developer);

        return mapper.Map<DeveloperResponse>(developer);
    }
}