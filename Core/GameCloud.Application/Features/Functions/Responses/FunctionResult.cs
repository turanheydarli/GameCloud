using System.Text.Json;
using GameCloud.Application.Features.Notifications.Requests;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Functions.Responses;


public record EntityAttributeUpdate(
    string Collection,
    string Key,
    string? Value
);