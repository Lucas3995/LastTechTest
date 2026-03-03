using LastTechTest.Aplicacao.Common.Responses;

using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthTokensDto>;