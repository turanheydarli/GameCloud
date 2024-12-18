using AutoMapper;
using GameCloud.Application.Common.Responses;
using GameCloud.Domain.Repositories;

namespace GameCloud.Application.Common.Mappings;

public class GeneralMappingProfile : Profile
{
    public GeneralMappingProfile()
    {
        CreateMap(typeof(IPaginate<>), typeof(PageableListResponse<>))
            .ConvertUsing(typeof(PaginateToPageableListConverter<,>));
    }
}