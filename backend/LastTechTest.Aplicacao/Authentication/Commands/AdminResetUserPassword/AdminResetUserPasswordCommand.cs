using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;

public sealed record AdminResetUserPasswordCommand(string? Email, Guid? Id) : IRequest<Unit>;