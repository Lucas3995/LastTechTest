using LastTechTest.Aplicacao.Common.Responses;
using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password) : IRequest<AuthTokensDto>;

