using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Sessions.Responses;

public record SessionResponse(Guid Id, Guid GameId, SessionStatus Status);