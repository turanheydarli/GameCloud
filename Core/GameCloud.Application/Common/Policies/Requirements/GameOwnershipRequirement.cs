using Microsoft.AspNetCore.Authorization;

namespace GameCloud.Application.Common.Policies.Requirements;

public class GameOwnershipRequirement : IAuthorizationRequirement
{
}

public class FunctionOwnershipRequirement : IAuthorizationRequirement
{
}