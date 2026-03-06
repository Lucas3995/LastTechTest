namespace LastTechTest.Aplicacao.Authentication.Queries.ListUsers;

public sealed record ListUsersResponse(IReadOnlyList<ListUsersItemResponse> Items);
