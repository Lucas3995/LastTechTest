using LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastTechTest.API.Controllers;

[ApiController]
[Route("user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender) => _sender = sender;

    [HttpGet("logged")]
    public async Task<IActionResult> GetLogged(CancellationToken ct)
    {
        try
        {
            var result = await _sender.Send(new GetLoggedUserQuery(), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return ExceptionMapping.ToActionResult(ex);
        }
    }
}