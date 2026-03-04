using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.AdminCreateUser;

public sealed record AdminCreateUserCommand(string Email, IReadOnlyCollection<string> Roles) : IRequest<Guid>;

