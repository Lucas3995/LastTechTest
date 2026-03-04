using LastTechTest.Aplicacao.Common.Extensions;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Interfaces;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;

public sealed class GetAnticipationRequestByIdQueryHandler : IRequestHandler<GetAnticipationRequestByIdQuery, GetAnticipationRequestByIdResponse?>
{
    private readonly IAnticipationRequestRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAnticipationRequestByIdQueryHandler(
        IAnticipationRequestRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<GetAnticipationRequestByIdResponse?> Handle(GetAnticipationRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.EnsureAuthenticated();

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return null;

        var role = _currentUser.GetRole();
        if (role != KnownRoles.Admin && entity.CreatorId != userId)
            throw new UnauthorizedAccessException("You are not allowed to access this request.");

        return new GetAnticipationRequestByIdResponse(
            entity.Id,
            entity.Protocol,
            entity.CreatorId,
            entity.Status.ToString(),
            entity.RequestedAmount,
            entity.GrossAmount,
            entity.FeesAmount,
            entity.NetAmount,
            entity.RequestedAtUtc,
            entity.CreatedAtUtc);
    }
}