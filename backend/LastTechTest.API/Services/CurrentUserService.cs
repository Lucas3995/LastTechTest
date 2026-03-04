using System.Security.Claims;

using LastTechTest.Aplicacao.Common.Interfaces;

using Microsoft.AspNetCore.Http;

namespace LastTechTest.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Claims is null) return null;
        var sub = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                  ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public string? GetRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Claims is null) return null;
        return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
    }
}
