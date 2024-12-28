using AutoMapper;
using GameCloud.Application.Features.Sessions.Requests;
using GameCloud.Application.Features.Sessions.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Sessions;

public class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<SessionRequest, Session>();
        CreateMap<Session, SessionResponse>();
    }
}