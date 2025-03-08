using AutoMapper;
using GameCloud.Application.Features.Rooms.Requests;
using GameCloud.Application.Features.Rooms.Responses;
using GameCloud.Domain.Entities.Rooms;

namespace GameCloud.Application.Features.Rooms;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<CreateRoomRequest, Room>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.State, opt => opt.MapFrom(_ => RoomState.Created))
            .ForMember(dest => dest.PlayerIds, opt => opt.MapFrom(_ => new List<string>()))
            .ForMember(dest => dest.SpectatorIds, opt => opt.MapFrom(_ => new List<string>()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.CurrentTurnUserId, opt => opt.Ignore())
            .ForMember(dest => dest.TurnNumber, opt => opt.MapFrom(_ => 0));

        CreateMap<RoomConfigRequest, RoomConfig>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<Room, RoomResponse>();
        CreateMap<RoomConfig, RoomConfigResponse>();
    }
} 