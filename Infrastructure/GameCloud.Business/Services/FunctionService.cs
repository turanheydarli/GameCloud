using System.Text;
using System.Text.Json;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class FunctionService : IFunctionService
{
    private readonly HttpClient _httpClient;
    private readonly IFunctionRepository _functionRepository;

    public FunctionService(HttpClient httpClient, IFunctionRepository functionRepository)
    {
        _httpClient = httpClient;
        _functionRepository = functionRepository;
    }

    public async Task<FunctionResponse> InvokeAsync(Guid id, JsonDocument parameters)
    {
        var function = await _functionRepository.GetByIdAsync(id);

        if (function == null)
        {
            throw new NotFoundException("Function", id);
        }

        var httpContent = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");

        var httpResult = await _httpClient.PostAsync(function.Endpoint, httpContent);

        if (!httpResult.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to invoke function. Status code: {httpResult.StatusCode}");
        }

        var responseContent = await httpResult.Content.ReadAsStringAsync();
        var functionResponse = JsonSerializer.Deserialize<FunctionResponse>(responseContent);

        if (functionResponse == null)
        {
            throw new InvalidOperationException("Function response is invalid or empty.");
        }

        return functionResponse;
    }
}