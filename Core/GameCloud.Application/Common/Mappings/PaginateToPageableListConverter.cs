using AutoMapper;
using GameCloud.Application.Common.Responses;
using GameCloud.Domain.Repositories;

namespace GameCloud.Application.Common.Mappings;

public class PaginateToPageableListConverter<TSource, TDestination>
    : ITypeConverter<IPaginate<TSource>, PageableListResponse<TDestination>>
{
    private readonly IMapper _mapper;

    public PaginateToPageableListConverter(IMapper mapper)
    {
        _mapper = mapper;
    }

    public PageableListResponse<TDestination> Convert(
        IPaginate<TSource> source,
        PageableListResponse<TDestination> destination,
        ResolutionContext context)
    {
        return new PageableListResponse<TDestination>(
            source.Index,
            source.Size,
            source.Count,
            source.Pages,
            source.HasPrevious,
            source.HasNext,
            _mapper.Map<IList<TDestination>>(source.Items)
        );
    }
}