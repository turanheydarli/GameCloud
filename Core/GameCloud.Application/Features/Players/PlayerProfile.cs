using AutoMapper;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Players;

public class PlayerProfile : Profile
{
    public PlayerProfile()
    {
        CreateMap<PlayerRequest, Player>();
        CreateMap<Player, PlayerResponse>();
    }
}