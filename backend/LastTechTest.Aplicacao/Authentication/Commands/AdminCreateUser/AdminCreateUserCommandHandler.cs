using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Commands.AdminCreateUser;

public sealed class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public AdminCreateUserCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    public async Task<Guid> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (request.Roles is null || request.Roles.Count == 0)
        {
            throw new InvalidOperationException("At least one role must be provided.");
        }

        var normalizedEmail = request.Email.Trim();

        var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            KnownRoles.Admin,
            KnownRoles.Creator,
            KnownRoles.Analista
        };

        var distinctRoles = request.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (distinctRoles.Length == 0)
        {
            throw new InvalidOperationException("At least one valid role must be provided.");
        }

        if (distinctRoles.Any(r => !allowedRoles.Contains(r)))
        {
            throw new InvalidOperationException("One or more roles are invalid.");
        }

        var existingDomainUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingDomainUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        const string defaultPassword = "Trocar@123";

        var identityUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (identityUser is null)
        {
            identityUser = new IdentityUser<Guid>
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create identity user: " +
                                                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        var userRoles = await _userManager.GetRolesAsync(identityUser);
        var rolesToAdd = distinctRoles
            .Except(userRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesToAdd.Length > 0)
        {
            var addRolesResult = await _userManager.AddToRolesAsync(identityUser, rolesToAdd);
            if (!addRolesResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to assign roles to user: " +
                                                    string.Join(", ", addRolesResult.Errors.Select(e => e.Description)));
            }
        }

        var domainUser = new User();
        domainUser.SetEmail(normalizedEmail);
        var hash = _passwordHasher.HashPassword(domainUser, defaultPassword);
        domainUser.SetPasswordHash(hash);

        await _userRepository.AddAsync(domainUser, cancellationToken);

        return domainUser.Id;
    }
}

