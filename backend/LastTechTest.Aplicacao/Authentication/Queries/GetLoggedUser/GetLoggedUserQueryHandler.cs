using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;

using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;

public sealed class GetLoggedUserQueryHandler : IRequestHandler<GetLoggedUserQuery, GetLoggedUserResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetLoggedUserQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<GetLoggedUserResponse> Handle(GetLoggedUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            throw new InvalidOperationException("No logged user in context.");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return new GetLoggedUserResponse(user.Id, user.Email, user.Status.ToString());
    }
}