using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Dominio;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;

public sealed class CancelAnticipationRequestCommandHandler : IRequestHandler<CancelAnticipationRequestCommand, CancelAnticipationRequestResponse>
{
    private readonly IAnticipationTransitionExecutor _executor;

    public CancelAnticipationRequestCommandHandler(IAnticipationTransitionExecutor executor)
    {
        _executor = executor;
    }

    public async Task<CancelAnticipationRequestResponse> Handle(CancelAnticipationRequestCommand request, CancellationToken cancellationToken)
    {
        var result = await _executor.ExecuteAsync(
            request.RequestId,
            AnticipationTransitionAction.Cancel,
            request.Reason,
            cancellationToken);
        var entity = result.Entity;
        return new CancelAnticipationRequestResponse(entity.Id, entity.Protocol, entity.Status, result.AlreadyCanceled);
    }
}