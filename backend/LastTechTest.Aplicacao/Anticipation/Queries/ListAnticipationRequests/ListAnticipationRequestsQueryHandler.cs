using LastTechTest.Aplicacao.Common.Extensions;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;

public sealed class ListAnticipationRequestsQueryHandler : IRequestHandler<ListAnticipationRequestsQuery, ListAnticipationRequestsResponse>
{
    private readonly IAnticipationRequestRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public ListAnticipationRequestsQueryHandler(
        IAnticipationRequestRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ListAnticipationRequestsResponse> Handle(ListAnticipationRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.EnsureAuthenticated();

        var role = _currentUser.GetRole();
        var hasGlobalReadAccess = role == KnownRoles.Admin || role == KnownRoles.Analista;
        Guid? creatorFilter = hasGlobalReadAccess ? request.CreatorId : userId;

        var statusFilter = request.Status.HasValue && Enum.IsDefined(typeof(AnticipationRequestStatus), request.Status.Value)
            ? (AnticipationRequestStatus?)request.Status.Value
            : null;

        var filter = new ListAnticipationRequestsFilter(
            creatorFilter,
            statusFilter,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize);

        var (items, totalCount) = await _repository.ListAsync(filter, cancellationToken);

        var list = items.Select(e => new AnticipationRequestListItem(
            e.Id,
            e.Protocol,
            e.CreatorId,
            e.Status.ToString(),
            e.RequestedAmount,
            e.NetAmount,
            e.RequestedAtUtc,
            e.CreatedAtUtc)).ToList();

        return new ListAnticipationRequestsResponse(list, totalCount);
    }
}
