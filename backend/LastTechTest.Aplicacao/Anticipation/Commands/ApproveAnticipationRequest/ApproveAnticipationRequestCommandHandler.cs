using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Dominio;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;

public sealed class ApproveAnticipationRequestCommandHandler : IRequestHandler<ApproveAnticipationRequestCommand, ApproveAnticipationRequestResponse>
{
    private readonly IAnticipationTransitionExecutor _executor;

    public ApproveAnticipationRequestCommandHandler(IAnticipationTransitionExecutor executor)
    {
        _executor = executor;
    }

    public async Task<ApproveAnticipationRequestResponse> Handle(ApproveAnticipationRequestCommand request, CancellationToken cancellationToken)
    {
        var result = await _executor.ExecuteAsync(
            request.RequestId,
            AnticipationTransitionAction.Approve,
            request.Observation,
            cancellationToken);
        var entity = result.Entity;
        return new ApproveAnticipationRequestResponse(entity.Id, entity.Protocol, entity.Status);
    }
}
