using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>;

