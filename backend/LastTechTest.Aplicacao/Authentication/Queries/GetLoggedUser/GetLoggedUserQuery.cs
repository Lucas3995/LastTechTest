using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;

public sealed record GetLoggedUserQuery : IRequest<GetLoggedUserResponse>;