namespace LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;

public sealed record GetLoggedUserResponse(Guid Id, string Email, string Status);

