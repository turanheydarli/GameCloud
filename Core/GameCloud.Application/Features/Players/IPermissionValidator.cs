using System.Text.Json;

namespace GameCloud.Application.Features.Players;

public interface IPermissionValidator
{
    Task<bool> HasPermission(string username, JsonDocument permissions);
}