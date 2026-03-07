using LastTechTest.Dominio.Authorization;

namespace LastTechTest.Dominio.Services;

public static class CreatorResolution
{
    public static Guid? ResolveCreatorId(Guid? requestCreatorId, Guid currentUserId, string? role)
    {
        if (string.Equals(role, KnownRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, KnownRoles.Analista, StringComparison.OrdinalIgnoreCase))
            return requestCreatorId ?? currentUserId;

        if (string.Equals(role, KnownRoles.Creator, StringComparison.OrdinalIgnoreCase))
            return requestCreatorId.HasValue && requestCreatorId.Value != currentUserId ? null : currentUserId;

        return currentUserId;
    }
}