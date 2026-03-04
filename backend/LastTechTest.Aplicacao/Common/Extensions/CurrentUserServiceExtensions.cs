using LastTechTest.Aplicacao.Common.Interfaces;

namespace LastTechTest.Aplicacao.Common.Extensions;

/// <summary>Extensões para ICurrentUserService (ex.: garantir utilizador autenticado).</summary>
public static class CurrentUserServiceExtensions
{
    /// <summary>Retorna o id do utilizador atual ou lança UnauthorizedAccessException se não autenticado.</summary>
    public static Guid EnsureAuthenticated(this ICurrentUserService currentUser)
    {
        var userId = currentUser.GetCurrentUserId();
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated.");
        return userId.Value;
    }
}