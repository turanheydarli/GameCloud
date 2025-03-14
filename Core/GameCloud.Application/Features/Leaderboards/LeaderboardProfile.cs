using AutoMapper;
using GameCloud.Application.Features.Leaderboards.Requests;
using GameCloud.Application.Features.Leaderboards.Responses;
using GameCloud.Domain.Entities.Leaderboards;

namespace GameCloud.Application.Features.Leaderboards;

public class LeaderboardProfile : Profile
{
    public LeaderboardProfile()
    {
        CreateMap<Leaderboard, LeaderboardResponse>()
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder.ToString().ToLower()))
            .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => src.Operator.ToString().ToLower()));
            
        CreateMap<LeaderboardRequest, Leaderboard>()
            .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Operator, opt => opt.Ignore())
            .ForMember(dest => dest.Game, opt => opt.Ignore())
            .ForMember(dest => dest.Records, opt => opt.Ignore())
            .ForMember(dest => dest.Archives, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.GameId, opt => opt.Ignore())
            .ForMember(dest => dest.ResetStrategy, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategory, opt => opt.Ignore());

        // LeaderboardRecord mappings
        CreateMap<LeaderboardRecord, LeaderboardRecordResponse>();
        
        CreateMap<LeaderboardRecordRequest, LeaderboardRecord>()
            .ForMember(dest => dest.Leaderboard, opt => opt.Ignore())
            .ForMember(dest => dest.Player, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rank, opt => opt.Ignore())
            .ForMember(dest => dest.UpdateCount, opt => opt.Ignore());

        CreateMap<LeaderboardArchive, LeaderboardArchiveResponse>();
    }
}