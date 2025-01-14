namespace GameCloud.Application.Features.Games.Responses;

public record GameActivityResponse(
    string EventType,
    DateTime Timestamp,
    string Details
);