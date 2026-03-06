using LastTechTest.Aplicacao.Authentication.Commands.AdminCreateUser;
using LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;
using LastTechTest.Aplicacao.Authentication.Commands.ChangePassword;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.Logout;
using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;
using LastTechTest.Aplicacao.Authentication.Queries.ListUsers;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastTechTest.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("admin/users")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateUserCommand command, CancellationToken ct)
    {
        try
        {
            var id = await _sender.Send(command, ct);
            return Created($"/auth/admin/users/{id}", new { Id = id });
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("admin/users/reset-password")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> AdminResetUserPassword([FromBody] AdminResetUserPasswordCommand command, CancellationToken ct)
    {
        try
        {
            await _sender.Send(command, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpGet("admin/users")]
    [Authorize(Roles = LastTechTest.Dominio.Authorization.KnownRoles.Admin)]
    public async Task<IActionResult> AdminListUsers(CancellationToken ct)
    {
        var result = await _sender.Send(new ListUsersQuery(), ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(command, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        try
        {
            await _sender.Send(command, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(command, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }

    [HttpDelete("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        await _sender.Send(command, ct);
        return NoContent();
    }
}