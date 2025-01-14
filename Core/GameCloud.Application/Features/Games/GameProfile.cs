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

        CreateMap<Game, GameDetailResponse>()
            .ForMember(dest => dest.ImageUrl,
                opt => opt.MapFrom(src => src.Image == null ? null : src.Image.Url))
            .ForMember(dest => dest.TotalPlayerCount,
                opt => opt.MapFrom(src => src.Players.Count))
            .ForMember(dest => dest.ActivePlayerCount,
                opt => opt.MapFrom(src => src.Players.Count(p => p.UpdatedAt >= DateTime.UtcNow.AddDays(-30))))
            .ForMember(dest => dest.FunctionCount,
                opt => opt.MapFrom(src => src.Functions.Count))
            .ForMember(dest => dest.KeyCount,
                opt => opt.MapFrom(src => src.GameKeys.Count))
            .ForMember(dest => dest.RecentActivity,
                opt => opt.MapFrom(src => src.Activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .Select(a => new GameActivityResponse(a.EventType, a.Timestamp, a.Details))));

        CreateMap<GameKey, GameKeyResponse>();
    }
}