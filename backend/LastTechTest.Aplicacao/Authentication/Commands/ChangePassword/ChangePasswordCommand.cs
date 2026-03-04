using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>;

