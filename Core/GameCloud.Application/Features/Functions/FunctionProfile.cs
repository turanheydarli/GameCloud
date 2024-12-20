using AutoMapper;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Functions;

public class FunctionProfile : Profile
{
    public FunctionProfile()
    {
        CreateMap<FunctionRequest, FunctionConfig>();
        CreateMap<FunctionConfig, FunctionResponse>();
    }
}