using AutoMapper;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.ImageDocuments;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<ImageDocument, ImageResponse>();
    }
}
