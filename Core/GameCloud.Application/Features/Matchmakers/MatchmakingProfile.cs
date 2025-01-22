using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Features.Matchmakers.Requests;
using GameCloud.Application.Features.Matchmakers.Responses;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Application.Features.Matchmakers;

public class MatchmakingProfile : Profile
{
    public MatchmakingProfile()
    {
        CreateMap<MatchmakingQueue, MatchmakingResponse>();
        
        CreateMap<MatchTicket, MatchTicketResponse>();

        CreateMap<Match, MatchResponse>();

        CreateMap<MatchQueueRequest, MatchmakingQueue>()
            .ForMember(dest => dest.Criteria, opt =>
                opt.MapFrom(src =>
                    JsonSerializer.SerializeToDocument(src.Criteria, new JsonSerializerOptions())
                ));
    }
}