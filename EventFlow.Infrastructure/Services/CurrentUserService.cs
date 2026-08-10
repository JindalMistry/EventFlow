using System.Security.Claims;
using EventFlow.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventFlow.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier) ?? user?.FindFirst("UserId");
            if (claim != null && Guid.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }

    public string Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(ClaimTypes.Role) ?? user?.FindFirst("Role");
            return claim?.Value ?? string.Empty;
        }
    }

    public Guid? ApplicationId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("ApplicationId");
            if (claim != null && Guid.TryParse(claim.Value, out var appId))
            {
                return appId;
            }
            return null;
        }
    }

    public bool IsRootAdmin => Role.Equals("RootAdmin", StringComparison.OrdinalIgnoreCase);
    public bool IsApplicationAdmin => Role.Equals("ApplicationAdmin", StringComparison.OrdinalIgnoreCase);
}
