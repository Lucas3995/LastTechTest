using FluentValidation;

using LastTechTest.Aplicacao.Common.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace LastTechTest.API;

/// <summary>Maps exceptions to HTTP results (status and body). Single responsibility for exception-to-response mapping.</summary>
public static class ExceptionMapping
{
    public static IResult MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException => Results.Json(new { error = ex.Message }, statusCode: 404),
            UnauthorizedAccessException => Results.Json(new { error = ex.Message }, statusCode: 401),
            InvalidOperationException => Results.BadRequest(new { error = ex.Message }),
            ValidationException => Results.BadRequest(new { error = ex.Message }),
            _ => Results.Json(new { error = "An error occurred." }, statusCode: 500)
        };
    }

    /// <summary>Returns an IActionResult for use in MVC controllers (same mapping as MapException).</summary>
    public static IActionResult ToActionResult(Exception ex)
    {
        return ex switch
        {
            NotFoundException => new ObjectResult(new { error = ex.Message }) { StatusCode = 404 },
            UnauthorizedAccessException => new ObjectResult(new { error = ex.Message }) { StatusCode = 401 },
            InvalidOperationException => new BadRequestObjectResult(new { error = ex.Message }),
            ValidationException => new BadRequestObjectResult(new { error = ex.Message }),
            _ => new ObjectResult(new { error = "An error occurred." }) { StatusCode = 500 }
        };
    }
}