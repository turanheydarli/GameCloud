using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Functioning.Functions;

public class HttpFunctionExecutor(IHttpClientFactory httpClientFactory) : IFunctionExecutor
{
    public async Task<FunctionResult?> InvokeAsync(FunctionInvokeRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var httpClient = httpClientFactory.CreateClient("Functions");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        string payloadJson = JsonSerializer.Serialize(request, jsonOptions);
        using var httpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await httpClient.PostAsync(request.Endpoint, httpContent);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException(
                $"Network error while calling function endpoint '{request.Endpoint}'. Error: {ex.Message}",
                ex
            );
        }

        stopwatch.Stop();

        if (!httpResponse.IsSuccessStatusCode)
        {
            var rawError = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Function call failed to '{request.Endpoint}'. " +
                $"Status code: {httpResponse.StatusCode}, Response: {rawError}"
            );
        }

        var responseJson = await httpResponse.Content.ReadAsStringAsync();

        FunctionResult? functionResult;
        try
        {
            functionResult = JsonSerializer.Deserialize<FunctionResult>(responseJson, jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize function response from '{request.Endpoint}'. Raw: {responseJson}",
                ex
            );
        }

        if (functionResult?.Status != FunctionStatus.Success)
        {
            throw new FunctionResultException(functionResult!);
        }

        if (functionResult == null)
        {
            throw new InvalidOperationException(
                $"The function response from '{request.Endpoint}' was null or invalid. Raw: {responseJson}"
            );
        }

        Console.WriteLine($"[HttpFunctionExecutor] Called {request.Endpoint} in {stopwatch.ElapsedMilliseconds} ms");

        return functionResult;
    }
}