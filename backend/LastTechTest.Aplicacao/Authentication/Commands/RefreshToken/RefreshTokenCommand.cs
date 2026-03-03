using LastTechTest.Aplicacao.Common.Responses;

using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthTokensDto>;