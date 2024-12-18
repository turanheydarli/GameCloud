using System.Text.Json.Serialization;

namespace GameCloud.Application.Features.Games.Requests;

public record GameRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("developer_id")] Guid DeveloperId
);