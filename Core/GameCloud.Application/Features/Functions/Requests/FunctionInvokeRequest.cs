using System.Text.Json;

namespace GameCloud.Application.Features.Functions.Requests;

public record FunctionInvokeRequest(
    string Endpoint,
    Guid SessionId,
    JsonDocument Payload
);