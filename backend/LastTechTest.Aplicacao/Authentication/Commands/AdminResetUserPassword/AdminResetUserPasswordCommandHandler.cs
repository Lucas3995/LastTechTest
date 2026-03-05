using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;

public sealed class AdminResetUserPasswordCommandHandler : IRequestHandler<AdminResetUserPasswordCommand, Unit>
{
    private const string DefaultPassword = "Trocar@123";

    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public AdminResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var domainUser = await ResolveDomainUserAsync(request, cancellationToken);
        if (domainUser is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var identityUser = await _userManager.FindByEmailAsync(domainUser.Email);
        if (identityUser is null)
        {
            throw new InvalidOperationException("User identity not found.");
        }

        var removeResult = await _userManager.RemovePasswordAsync(identityUser);
        if (!removeResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to remove password: " +
                string.Join(", ", removeResult.Errors.Select(e => e.Description)));
        }

        var addResult = await _userManager.AddPasswordAsync(identityUser, DefaultPassword);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to set password: " +
                string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }

        var hash = _passwordHasher.HashPassword(domainUser, DefaultPassword);
        domainUser.SetPasswordHash(hash);
        await _userRepository.UpdateAsync(domainUser, cancellationToken);

        return Unit.Value;
    }

    private async Task<LastTechTest.Dominio.Entities.User?> ResolveDomainUserAsync(
        AdminResetUserPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            return await _userRepository.GetByIdAsync(request.Id.Value, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            return await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        }

        return null;
    }
}