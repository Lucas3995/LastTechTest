using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var domainUser = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (domainUser is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var identityUser = await _userManager.FindByEmailAsync(domainUser.Email);
        if (identityUser is null)
        {
            throw new InvalidOperationException("User identity not found.");
        }

        var identityResult = await _userManager.ChangePasswordAsync(identityUser, request.CurrentPassword, request.NewPassword);
        if (!identityResult.Succeeded)
        {
            if (identityResult.Errors.Any(e => e.Code.Contains("PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Current password is invalid.");
            }

            var description = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to change password: {description}");
        }

        var newHash = _passwordHasher.HashPassword(domainUser, request.NewPassword);
        domainUser.SetPasswordHash(newHash);
        await _userRepository.UpdateAsync(domainUser, cancellationToken);

        return Unit.Value;
    }
}