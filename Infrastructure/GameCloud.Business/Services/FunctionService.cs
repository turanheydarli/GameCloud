using System.Text;
using System.Text.Json;
using AutoMapper;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Games.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class FunctionService(
    HttpClient httpClient,
    IFunctionRepository functionRepository,
    IGameRepository gameRepository,
    IMapper mapper) : IFunctionService
{
    public async Task<FunctionResponse> CreateFunctionAsync(Guid gameId, FunctionRequest request, Guid userId)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var functionConfig = new FunctionConfig
        {
            GameId = gameId,
            Name = request.Name,
            IsEnabled = true,
            ActionType = request.ActionType,
            Endpoint = request.Endpoint,
            CreatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid().ToString().Split('-')[0]
        };

        functionConfig = await functionRepository.CreateAsync(functionConfig);

        return mapper.Map<FunctionResponse>(functionConfig);
    }

    public async Task<PageableListResponse<FunctionResponse>> GetFunctionsAsync(Guid gameId, PageableRequest request)
    {
        var functions = await functionRepository.GetListPaginatedAsync(f => f.GameId == gameId);

        return mapper.Map<PageableListResponse<FunctionResponse>>(functions);
    }

    public async Task<FunctionResponse> UpdateAsync(Guid functionId, FunctionRequest request)
    {
        var function = await functionRepository.GetAsync(f => f.Id == functionId);

        if (function is null)
        {
            throw new NotFoundException("Function", functionId);
        }

        function.Version = Guid.NewGuid().ToString().Split('-')[0];

        function.Name = request.Name;
        function.IsEnabled = request.IsEnabled;
        function.ActionType = request.ActionType;
        function.Endpoint = request.Endpoint;
        function.UpdatedAt = DateTime.UtcNow;

        await functionRepository.UpdateAsync(function);

        return mapper.Map<FunctionResponse>(function);
    }

    public async Task<FunctionResponse> ToggleFunction(Guid gameId, Guid functionId, bool isEnabled)
    {
        var function = await functionRepository.GetAsync(f => f.Id == functionId);

        if (function is null)
        {
            throw new NotFoundException("Function", functionId);
        }

        function.IsEnabled = isEnabled;
        function.UpdatedAt = DateTime.UtcNow;

        await functionRepository.UpdateAsync(function);

        return mapper.Map<FunctionResponse>(function);
    }

    public async Task<FunctionResponse> GetById(Guid gameId, Guid functionId)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var function = await functionRepository.GetAsync(
            f => f.GameId == gameId && f.Id == functionId);

        if (function is null)
            throw new NotFoundException("Function", functionId);

        return mapper.Map<FunctionResponse>(function);
    }

    public async Task DeleteAsync(Guid functionId)
    {
        var function = await functionRepository.GetAsync(f => f.Id == functionId);

        if (function is null)
        {
            throw new NotFoundException("Function", functionId);
        }

        await functionRepository.DeleteAsync(function);
    }
}