using FluentValidation;

using LastTechTest.Aplicacao.Common.Exceptions;

namespace LastTechTest.API;

/// <summary>Maps exceptions to HTTP results (status and body). Single responsibility for exception-to-response mapping.</summary>
public static class ExceptionMapping
{
    public static IResult MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException => Results.Json(new { error = ex.Message }, statusCode: 404),
            UnauthorizedAccessException => Results.Json(new { error = ex.Message }, statusCode: 403),
            InvalidOperationException => Results.BadRequest(new { error = ex.Message }),
            ValidationException => Results.BadRequest(new { error = ex.Message }),
            _ => Results.Json(new { error = "An error occurred." }, statusCode: 500)
        };
    }
}