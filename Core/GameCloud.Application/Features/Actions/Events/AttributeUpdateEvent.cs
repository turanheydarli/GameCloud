using GameCloud.Application.Features.Functions.Responses;

namespace GameCloud.Application.Features.Actions.Events;

public record AttributeUpdateEvent(
    Guid UserId,
    string Username,
    List<EntityAttributeUpdate> Updates,
    string Source = "server",
    DateTime Timestamp = default);