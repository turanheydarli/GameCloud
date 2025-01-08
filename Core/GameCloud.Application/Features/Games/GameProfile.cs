using AutoMapper;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Games;

public class GameProfile : Profile
{
    public GameProfile()
    {
        CreateMap<GameRequest, Game>();

        CreateMap<Game, GameResponse>()
            .ForMember(dest => dest.ImageUrl,
                opt =>
                    opt.MapFrom(src => src.Image == null ? null : src.Image.Url));

        CreateMap<GameKey, GameKeyResponse>();
    }
}