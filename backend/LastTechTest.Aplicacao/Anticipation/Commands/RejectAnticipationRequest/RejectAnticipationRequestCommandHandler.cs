using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Dominio;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;

public sealed class RejectAnticipationRequestCommandHandler : IRequestHandler<RejectAnticipationRequestCommand, RejectAnticipationRequestResponse>
{
    private readonly IAnticipationTransitionExecutor _executor;

    public RejectAnticipationRequestCommandHandler(IAnticipationTransitionExecutor executor)
    {
        _executor = executor;
    }

    public async Task<RejectAnticipationRequestResponse> Handle(RejectAnticipationRequestCommand request, CancellationToken cancellationToken)
    {
        var result = await _executor.ExecuteAsync(
            request.RequestId,
            AnticipationTransitionAction.Reject,
            request.Reason,
            cancellationToken);
        var entity = result.Entity;
        return new RejectAnticipationRequestResponse(entity.Id, entity.Protocol, entity.Status);
    }
}