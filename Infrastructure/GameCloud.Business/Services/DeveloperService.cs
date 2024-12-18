using AutoMapper;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class DeveloperService(IDeveloperRepository developerRepository, IMapper mapper) : IDeveloperService
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

    public async Task<DeveloperResponse> GetByUserIdAsync(Guid userId)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);

        if (developer is null)
        {
            throw new NotFoundException("User", userId);
        }

        return mapper.Map<DeveloperResponse>(developer);
    }
}