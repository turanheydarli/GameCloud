using AutoMapper;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Actions;

public class ActionProfile : Profile
{
    public ActionProfile()
    {
        CreateMap<ActionRequest, ActionLog>();

        CreateMap<ActionLog, ActionResponse>();
    }
}