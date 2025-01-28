using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Domain.Entities;

public record FunctionResult(
    [field: JsonPropertyName("data")] JsonDocument? Data,
    [field: JsonPropertyName("isSuccess")] bool IsSuccess,
    [field: JsonPropertyName("errorMessage")] string? ErrorMessage = null
);