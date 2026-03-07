using System.Diagnostics;

using FluentValidation;

using LastTechTest.Aplicacao.Common.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace LastTechTest.API;

/// <summary>Maps exceptions to HTTP results (status and body). Single responsibility for exception-to-response mapping.</summary>
public static class ExceptionMapping
{
    public static IResult MapException(Exception ex)
    {
        var traceId = Activity.Current?.TraceId.ToString();

        return ex switch
        {
            NotFoundException => Results.Json(new { error = ex.Message, traceId }, statusCode: 404),
            UnauthorizedAccessException => Results.Json(new { error = ex.Message, traceId }, statusCode: 401),
            DomainValidationException => Results.BadRequest(new { error = ex.Message, traceId }),
            InvalidOperationException => Results.BadRequest(new { error = ex.Message, traceId }),
            ValidationException => Results.BadRequest(new { error = ex.Message, traceId }),
            _ => Results.Json(new { error = "An error occurred.", traceId }, statusCode: 500)
        };
    }

    /// <summary>Returns an IActionResult for use in MVC controllers (same mapping as MapException).</summary>
    public static IActionResult ToActionResult(Exception ex)
    {
        var traceId = Activity.Current?.TraceId.ToString();

        return ex switch
        {
            NotFoundException => new ObjectResult(new { error = ex.Message, traceId }) { StatusCode = 404 },
            UnauthorizedAccessException => new ObjectResult(new { error = ex.Message, traceId }) { StatusCode = 401 },
            DomainValidationException => new BadRequestObjectResult(new { error = ex.Message, traceId }),
            InvalidOperationException => new BadRequestObjectResult(new { error = ex.Message, traceId }),
            ValidationException => new BadRequestObjectResult(new { error = ex.Message, traceId }),
            _ => new ObjectResult(new { error = "An error occurred.", traceId }) { StatusCode = 500 }
        };
    }
}