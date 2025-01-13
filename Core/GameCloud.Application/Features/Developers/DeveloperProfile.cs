using AutoMapper;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Application.Features.Developers;

public class DeveloperProfile : Profile
{
    public DeveloperProfile()
    {
        CreateMap<DeveloperRequest, Developer>();
        CreateMap<Developer, DeveloperResponse>()
            .ForMember(m => m.ProfileImageUrl,
                opt 
                    => opt.MapFrom(p => p.ProfilePhoto.Url));
    }
}