using System.Text.Json;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Players;
using Microsoft.Extensions.Logging;

namespace GameCloud.Business.Services;

internal class PermissionRule
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

internal class PermissionValidator(ILogger<PermissionValidator> logger, ExecutionContextAccessor executionContextAccessor) : IPermissionValidator
{
    public async Task<bool> HasPermission(string username, JsonDocument permissions)
    {
        try
        {
            if (executionContextAccessor.Context == ActionExecutionContext.Function)
            {
                return true;
            }

            if (permissions.RootElement.ValueKind == JsonValueKind.Object &&
                !permissions.RootElement.EnumerateObject().Any())
            {
                return true;
            }

            var permissionDict = permissions.Deserialize<Dictionary<string, object>>();
            if (permissionDict == null)
            {
                logger.LogWarning("Invalid permission format for user {Username}", username);
                return false;
            }

            // Check for direct user permission
            if (permissionDict.ContainsKey("users"))
            {
                var users = JsonSerializer.Deserialize<List<string>>(
                    permissionDict["users"].ToString() ?? "[]");

                if (users != null && users.Contains(username))
                {
                    return true;
                }
            }

            // Check for role-based permission
            if (permissionDict.ContainsKey("roles"))
            {
                var roles = JsonSerializer.Deserialize<List<string>>(
                    permissionDict["roles"].ToString() ?? "[]");

                if (roles != null && await UserHasAnyRole(username, roles))
                {
                    return true;
                }
            }

            // Check for game-specific rules
            if (permissionDict.ContainsKey("rules"))
            {
                var rules = JsonSerializer.Deserialize<List<PermissionRule>>(
                    permissionDict["rules"].ToString() ?? "[]");

                if (rules != null && await EvaluateRules(username, rules))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating permissions for user {Username}", username);
            return false;
        }
    }

    private async Task<bool> UserHasAnyRole(string username, List<string> roles)
    {
        // TODO: Implement role checking against your user/role system
        // This is a placeholder implementation
        return roles.Contains("public");
    }

    private async Task<bool> EvaluateRules(string username, List<PermissionRule> rules)
    {
        foreach (var rule in rules)
        {
            if (await EvaluateRule(username, rule))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> EvaluateRule(string username, PermissionRule rule)
    {
        try
        {
            switch (rule.Type.ToLowerInvariant())
            {
                case "owner":
                    return username == rule.Value;

                case "team_member":
                    return await IsTeamMember(username, rule.Value);

                case "game_admin":
                    return await IsGameAdmin(username, rule.Value);

                case "attribute_condition":
                    return await CheckAttributeCondition(username, rule.Value);

                default:
                    logger.LogWarning(
                        "Unknown permission rule type: {RuleType} for user {Username}",
                        rule.Type, username);
                    return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error evaluating rule type {RuleType} for user {Username}",
                rule.Type, username);
            return false;
        }
    }

    private async Task<bool> IsTeamMember(string username, string teamId)
    {
        // TODO: Implement team membership check
        return false;
    }

    private async Task<bool> IsGameAdmin(string username, string gameId)
    {
        // TODO: Implement game admin check
        return false;
    }

    private async Task<bool> CheckAttributeCondition(string username, string condition)
    {
        // TODO: Implement attribute-based condition checking
        return false;
    }
}