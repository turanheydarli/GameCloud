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
    IMapper mapper)
    : IFunctionService
{
    public async Task<FunctionResult> InvokeAsync(Guid id, JsonDocument parameters)
    {
        var function = await functionRepository.GetByIdAsync(id);

        if (function == null)
        {
            throw new NotFoundException("Function", id);
        }

        var httpContent = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");

        var httpResult = await httpClient.PostAsync(function.Endpoint, httpContent);

        if (!httpResult.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to invoke function. Status code: {httpResult.StatusCode}");
        }

        var responseContent = await httpResult.Content.ReadAsStringAsync();
        var functionResponse = JsonSerializer.Deserialize<FunctionResult>(responseContent);

        if (functionResponse == null)
        {
            throw new InvalidOperationException("Function response is invalid or empty.");
        }

        return functionResponse;
    }

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
        };

        functionConfig = await functionRepository.CreateAsync(functionConfig);

        return mapper.Map<FunctionResponse>(functionConfig);
    }

    public Task<PageableListResponse<FunctionResponse>> GetFunctionsAsync(Guid gameId, PageableRequest request)
    {
        throw new NotImplementedException();
    }
}