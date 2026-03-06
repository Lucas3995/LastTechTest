using FluentValidation;

using LastTechTest.API.Dtos;
using LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;
using LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;
using LastTechTest.Aplicacao.Common.Exceptions;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastTechTest.API.Controllers;

[ApiController]
[Route("api/v1/anticipations")]
[Authorize]
public class AnticipationController : ControllerBase
{
    private readonly ISender _sender;

    public AnticipationController(ISender sender) => _sender = sender;

    [HttpGet("")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Creator + "," + LastTechTest.Dominio.Authorization.KnownRoles.Analista + "," + LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? creatorId,
        [FromQuery] int? status,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        try
        {
            var query = new ListAnticipationRequestsQuery(
                creatorId,
                status,
                fromUtc,
                toUtc,
                page ?? 1,
                pageSize ?? 20);
            var result = await _sender.Send(query, ct);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Creator + "," + LastTechTest.Dominio.Authorization.KnownRoles.Analista + "," + LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(new GetAnticipationRequestByIdQuery(id), ct);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("simulations")]
    [Authorize]
    public async Task<IActionResult> Simulate([FromBody] SimulateAnticipationRequestDto? body, CancellationToken ct)
    {
        if (body is null)
            return BadRequest(new { error = "Request body is required." });
        try
        {
            var command = new SimulateAnticipationRequestCommand(body.RequestedAmount, body.CreatorId, body.RequestedAtUtc);
            var result = await _sender.Send(command, ct);
            return Ok(new { result.SimulationCode, ValidUntilUtc = result.ValidUntilUtc, result.RequestedAmount, result.GrossAmount, result.FeesAmount, result.NetAmount });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Not implemented." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("simulations/{simulationCode}/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmSimulation(string simulationCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(simulationCode))
            return BadRequest(new { error = "Simulation code is required." });
        try
        {
            var result = await _sender.Send(new ConvertSimulationToRealRequestCommand(simulationCode), ct);
            return Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Not implemented." });
        }
        catch (NotFoundException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            var msg = ex.Message;
            if (msg.Contains("already used", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("open request", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("open anticipation", StringComparison.OrdinalIgnoreCase))
                return UnprocessableEntity(new { error = ex.Message });
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAnticipationRequestDto? body, CancellationToken ct)
    {
        if (body is null)
            return BadRequest(new { error = "Request body is required." });
        try
        {
            var command = new CreateAnticipationRequestCommand(body.RequestedAmount, body.CreatorId, body.RequestedAtUtc);
            var result = await _sender.Send(command, ct);
            return Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Analista + "," + LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveAnticipationRequestDto? body, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(new ApproveAnticipationRequestCommand(id, body?.Observation), ct);
            return Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Not implemented." });
        }
        catch (NotFoundException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Analista + "," + LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectAnticipationRequestDto? body, CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Reason))
            return BadRequest(new { error = "Reason is required." });
        try
        {
            var result = await _sender.Send(new RejectAnticipationRequestCommand(id, body.Reason), ct);
            return Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Not implemented." });
        }
        catch (NotFoundException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Creator + "," + LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAnticipationRequestDto? body, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(new CancelAnticipationRequestCommand(id, body?.Reason), ct);
            return Ok(new { result.Id, result.Protocol, Status = result.Status.ToString(), result.AlreadyCanceled });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Not implemented." });
        }
        catch (NotFoundException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }
}