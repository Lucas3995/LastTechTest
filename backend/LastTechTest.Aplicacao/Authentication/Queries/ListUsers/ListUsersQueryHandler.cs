using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Queries.ListUsers;

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, ListUsersResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public ListUsersQueryHandler(
        IUserRepository userRepository,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<ListUsersResponse> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var users = await _userRepository.ListAsync(cancellationToken);
        if (users.Count == 0)
        {
            return new ListUsersResponse([]);
        }

        var items = new List<ListUsersItemResponse>(users.Count);

        foreach (var user in users)
        {
            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            IReadOnlyList<string> roles = [];

            if (identityUser is not null)
            {
                roles = (await _userManager.GetRolesAsync(identityUser))
                    .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            items.Add(new ListUsersItemResponse(user.Id, user.Email, roles));
        }

        return new ListUsersResponse(items);
    }
}
