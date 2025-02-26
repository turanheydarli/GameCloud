using System.Security.Claims;
using GameCloud.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GameCloud.WebAPI.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    [Authorize]
    protected Guid GetUserIdFromClaims()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidUserClaimsException("User is not authenticated.");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            throw new InvalidUserClaimsException("Invalid token or missing user identifier claim.");
        }

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            throw new InvalidUserClaimsException("User identifier claim is not a valid GUID.");
        }

        return userId;
    }
    
    [Authorize]
    protected Guid GetPlayerIdFromClaims()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidUserClaimsException("User is not authenticated.");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            throw new InvalidUserClaimsException("Invalid token or missing user identifier claim.");
        }

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            throw new InvalidUserClaimsException("User identifier claim is not a valid GUID.");
        }

        return userId;
    }
    
    [Authorize]
    protected string GetUserNameFromClaims()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidUserClaimsException("User is not authenticated.");
        }

        var username = User.FindFirstValue("username");
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidUserClaimsException("Invalid token or missing user identifier claim.");
        }

        return username;
    }
}