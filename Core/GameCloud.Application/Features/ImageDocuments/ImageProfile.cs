using AutoMapper;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.ImageDocuments;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<ImageDocument, ImageResponse>()
            .ForMember(dest => dest.Url, 
                opt => opt.MapFrom(src => $"/api/v1/images/{src.Id}"))
            .ForMember(dest => dest.Variants, 
                opt => opt.MapFrom(src => src.Variants.Select(v => new ImageVariant
                {
                    Url = $"/api/v1/images/{src.Id}?variant={v.VariantType}",
                    Width = v.Width,
                    Height = v.Height,
                    VariantType = v.VariantType
                })));
    }
}
