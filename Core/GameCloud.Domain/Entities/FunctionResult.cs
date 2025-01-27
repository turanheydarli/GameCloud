using System.Text.Json;

namespace GameCloud.Domain.Entities;

public record FunctionResult(
    JsonDocument? Data,
    bool IsSuccess,
    string? ErrorMessage = null
);