using LastTechTest.API.Dtos;
using LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;
using LastTechTest.Aplicacao.Common.Exceptions;
using LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;
using LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using FluentValidation;

namespace LastTechTest.API.Endpoints;

public static class AnticipationEndpoints
{
    public static IEndpointRouteBuilder MapAnticipationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/anticipations");

        group.MapGet("", async (
                [FromQuery] Guid? creatorId,
                [FromQuery] int? status,
                [FromQuery] DateTime? fromUtc,
                [FromQuery] DateTime? toUtc,
                [FromQuery] int? page,
                [FromQuery] int? pageSize,
                [FromServices] ISender sender,
                CancellationToken ct) =>
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
                    var result = await sender.Send(query, ct);
                    return Results.Ok(new { result.Items, result.TotalCount });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
            })
            .RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

        group.MapGet("{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
            {
                try
                {
                    var result = await sender.Send(new GetAnticipationRequestByIdQuery(id), ct);
                    if (result is null)
                        return Results.NotFound();
                    return Results.Ok(result);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
            })
            .RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

        group.MapPost("simulations", async ([FromBody] SimulateAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
            {
                if (body is null)
                    return Results.BadRequest(new { error = "Request body is required." });
                try
                {
                    var command = new SimulateAnticipationRequestCommand(body.RequestedAmount, body.CreatorId, body.RequestedAtUtc);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(new { result.SimulationCode, ValidUntilUtc = result.ValidUntilUtc, result.RequestedAmount, result.GrossAmount, result.FeesAmount, result.NetAmount });
                }
                catch (NotImplementedException)
                {
                    return Results.Json(new { error = "Not implemented." }, statusCode: 501);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
                catch (InvalidOperationException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
            })
            .RequireAuthorization();

        group.MapPost("simulations/{simulationCode}/confirm", async (string simulationCode, [FromServices] ISender sender, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(simulationCode))
                    return Results.BadRequest(new { error = "Simulation code is required." });
                try
                {
                    var result = await sender.Send(new ConvertSimulationToRealRequestCommand(simulationCode), ct);
                    return Results.Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
                }
                catch (NotImplementedException)
                {
                    return Results.Json(new { error = "Not implemented." }, statusCode: 501);
                }
                catch (NotFoundException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
                catch (InvalidOperationException ex)
                {
                    var msg = ex.Message;
                    if (msg.Contains("already used", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("open request", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("open anticipation", StringComparison.OrdinalIgnoreCase))
                        return Results.Json(new { error = ex.Message }, statusCode: 422);
                    return ExceptionMapping.MapException(ex);
                }
            })
            .RequireAuthorization();

        group.MapPost("", async ([FromBody] CreateAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
            {
                if (body is null)
                    return Results.BadRequest(new { error = "Request body is required." });
                try
                {
                    var command = new CreateAnticipationRequestCommand(
                        body.RequestedAmount,
                        body.CreatorId,
                        body.RequestedAtUtc);
                    var result = await sender.Send(command, ct);
                    return Results.Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
                catch (InvalidOperationException ex)
                {
                    return ExceptionMapping.MapException(ex);
                }
            })
            .RequireAuthorization();

        group.MapPost("{id:guid}/approve", async (Guid id, [FromBody] ApproveAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
            {
                try
                {
                    var result = await sender.Send(new ApproveAnticipationRequestCommand(id, body?.Observation), ct);
                    return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
                }
                catch (NotImplementedException)
                {
                    return Results.Json(new { error = "Not implemented." }, statusCode: 501);
                }
                catch (NotFoundException ex) { return ExceptionMapping.MapException(ex); }
                catch (UnauthorizedAccessException ex) { return ExceptionMapping.MapException(ex); }
                catch (InvalidOperationException ex) { return ExceptionMapping.MapException(ex); }
            })
            .RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Analista, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

        group.MapPost("{id:guid}/reject", async (Guid id, [FromBody] RejectAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
            {
                if (body is null || string.IsNullOrWhiteSpace(body.Reason))
                    return Results.BadRequest(new { error = "Reason is required." });
                try
                {
                    var result = await sender.Send(new RejectAnticipationRequestCommand(id, body.Reason), ct);
                    return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
                }
                catch (NotImplementedException)
                {
                    return Results.Json(new { error = "Not implemented." }, statusCode: 501);
                }
                catch (NotFoundException ex) { return ExceptionMapping.MapException(ex); }
                catch (UnauthorizedAccessException ex) { return ExceptionMapping.MapException(ex); }
                catch (InvalidOperationException ex) { return ExceptionMapping.MapException(ex); }
            })
            .RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Analista, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

        group.MapPost("{id:guid}/cancel", async (Guid id, [FromBody] CancelAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
            {
                try
                {
                    var result = await sender.Send(new CancelAnticipationRequestCommand(id, body?.Reason), ct);
                    return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString(), result.AlreadyCanceled });
                }
                catch (NotImplementedException)
                {
                    return Results.Json(new { error = "Not implemented." }, statusCode: 501);
                }
                catch (NotFoundException ex) { return ExceptionMapping.MapException(ex); }
                catch (UnauthorizedAccessException ex) { return ExceptionMapping.MapException(ex); }
                catch (InvalidOperationException ex) { return ExceptionMapping.MapException(ex); }
            })
            .RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

        return app;
    }
}
