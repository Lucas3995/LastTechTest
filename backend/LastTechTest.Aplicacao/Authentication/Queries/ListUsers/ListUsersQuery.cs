using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Queries.ListUsers;

public sealed record ListUsersQuery : IRequest<ListUsersResponse>;
