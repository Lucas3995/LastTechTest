namespace LastTechTest.Aplicacao.Authentication.Queries.ListUsers;

public sealed record ListUsersItemResponse(Guid Id, string Email, IReadOnlyList<string> Roles);