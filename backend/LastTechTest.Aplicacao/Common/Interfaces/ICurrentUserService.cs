namespace LastTechTest.Aplicacao.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    string? GetRole();
}