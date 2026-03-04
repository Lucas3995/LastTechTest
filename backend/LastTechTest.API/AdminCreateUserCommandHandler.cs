using LastTechTest.Aplicacao.Authentication.Commands.AdminCreateUser;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.API;

public sealed class AdminCreateUserCommandHandler : IRequestHandler<AdminCreateUserCommand, Guid>
{
    private static readonly string[] AllowedRoles =
    {
        KnownRoles.Admin,
        KnownRoles.Creator,
        KnownRoles.Analista
    };

    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;

    public AdminCreateUserCommandHandler(
        UserManager<IdentityUser<Guid>> userManager,
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(AdminCreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        var invalidRoles = request.Roles
            .Where(r => !AllowedRoles.Contains(r, StringComparer.Ordinal))
            .ToArray();

        if (invalidRoles.Length > 0)
        {
            throw new InvalidOperationException("One or more roles are invalid.");
        }

        var existingIdentity = await _userManager.FindByEmailAsync(request.Email);
        if (existingIdentity is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        const string defaultPassword = "Trocar@123";

        var identityUser = new IdentityUser<Guid>
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create user: " +
                                                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        if (request.Roles.Count > 0)
        {
            var addToRolesResult = await _userManager.AddToRolesAsync(identityUser, request.Roles);
            if (!addToRolesResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to assign roles: " +
                                                    string.Join(", ", addToRolesResult.Errors.Select(e => e.Description)));
            }
        }

        var existingDomainUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingDomainUser is null)
        {
            var domainUser = new User();
            domainUser.SetEmail(request.Email);

            var hash = _passwordHasher.HashPassword(domainUser, defaultPassword);
            domainUser.SetPasswordHash(hash);

            await _userRepository.AddAsync(domainUser, cancellationToken);
        }

        return identityUser.Id;
    }
}

