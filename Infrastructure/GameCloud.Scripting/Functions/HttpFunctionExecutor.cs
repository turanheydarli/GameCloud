using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GameCloud.Functioning.Functions;

public class HttpFunctionExecutor : IFunctionExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpFunctionExecutor> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpFunctionExecutor(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpFunctionExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public async Task<FunctionResult> InvokeAsync(FunctionInvokeRequest request)
    {
        using var scope = _logger.BeginScope(new[]
        {
            new KeyValuePair<string, object>("FunctionEndpoint", request.Endpoint),
            new KeyValuePair<string, object>("SessionId", request.SessionId)
        });

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("[Function Call] Starting execution for {Endpoint}", request.Endpoint);
            _logger.LogDebug("Request payload: {Payload}", JsonSerializer.Serialize(request.Payload, _jsonOptions));

            using var httpClient = _httpClientFactory.CreateClient("Functions");
            var payloadJson = JsonSerializer.Serialize(request.Payload, _jsonOptions);
            using var httpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            LogHttpRequestDetails(httpClient, request.Endpoint, httpContent);

            var response = await ExecuteHttpRequest(httpClient, request.Endpoint, httpContent);
            var result = await ProcessHttpResponse(response, request.Endpoint);

            stopwatch.Stop();

            _logger.LogInformation("[Function Call] Completed successfully in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            _logger.LogDebug("Function result: {Result}", JsonSerializer.Serialize(result, _jsonOptions));

            return result;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[Function Call] HTTP error after {ElapsedMs}ms - Status: {StatusCode}",
                stopwatch.ElapsedMilliseconds, ex.StatusCode ?? HttpStatusCode.InternalServerError);
            throw new ApplicationException($"HTTP request failed: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[Function Call] JSON serialization error after {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            throw new ApplicationException("Invalid response format from function", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogCritical(ex, "[Function Call] Unexpected error after {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            throw new ApplicationException("Function execution failed", ex);
        }
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequest(HttpClient client, string endpoint, HttpContent content)
    {
        try
        {
            _logger.LogDebug("Sending POST request to {Endpoint}", endpoint);
            return await client.PostAsync(endpoint, content);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("Request timeout after {Timeout}s", client.Timeout.TotalSeconds);
            throw new HttpRequestException($"Request timed out after {client.Timeout.TotalSeconds}s", ex);
        }
    }

    private async Task<FunctionResult> ProcessHttpResponse(HttpResponseMessage response, string endpoint)
    {
        _logger.LogDebug("Received response: {StatusCode} ({ReasonPhrase})",
            (int)response.StatusCode, response.ReasonPhrase);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Function error response: {ErrorContent}", errorContent);

            throw new HttpRequestException(
                $"Function call failed with status {response.StatusCode}. Response: {errorContent}");
        }

        var responseStream = await response.Content.ReadAsStreamAsync();

        try
        {
            var result = await JsonSerializer.DeserializeAsync<FunctionResult>(
                responseStream, _jsonOptions);

            if (result == null)
            {
                _logger.LogError("Received null result from function endpoint");
                throw new InvalidOperationException("Null response from function endpoint");
            }

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Function returned non-success result: {ErrorMessage}", result.ErrorMessage);
            }

            return result;
        }
        catch (JsonException ex)
        {
            responseStream.Position = 0;
            var rawResponse = await new StreamReader(responseStream).ReadToEndAsync();
            _logger.LogError(ex, "Failed to deserialize response. Raw content: {RawResponse}", rawResponse);
            throw;
        }
    }

    private void LogHttpRequestDetails(HttpClient client, string endpoint, HttpContent content)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var headers = client.DefaultRequestHeaders
                .Select(h => $"{h.Key}: {string.Join(", ", h.Value)}");

            _logger.LogDebug("""
                             HTTP Request Details:
                             Endpoint: {Endpoint}
                             Headers: {Headers}
                             Content-Type: {ContentType}
                             Content-Length: {ContentLength} bytes
                             Timeout: {Timeout}s
                             """,
                endpoint,
                string.Join("; ", headers),
                content.Headers.ContentType?.MediaType,
                content.Headers.ContentLength,
                client.Timeout.TotalSeconds);
        }
    }
}