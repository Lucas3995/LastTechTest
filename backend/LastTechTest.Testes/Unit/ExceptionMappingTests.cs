using FluentAssertions;

using FluentValidation;

using LastTechTest.API;
using LastTechTest.Aplicacao.Common.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public sealed class ExceptionMappingTests
{
    private static async Task<int> GetStatusCodeAsync(IResult result)
    {
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        await result.ExecuteAsync(context);
        return context.Response.StatusCode;
    }

    [Fact]
    public async Task MapException_NotFoundException_Returns_404()
    {
        var ex = new NotFoundException("Not found.");
        var result = ExceptionMapping.MapException(ex);
        (await GetStatusCodeAsync(result)).Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task MapException_UnauthorizedAccessException_Returns_403()
    {
        var ex = new UnauthorizedAccessException("Forbidden.");
        var result = ExceptionMapping.MapException(ex);
        (await GetStatusCodeAsync(result)).Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task MapException_InvalidOperationException_Returns_400()
    {
        var ex = new InvalidOperationException("Bad request.");
        var result = ExceptionMapping.MapException(ex);
        (await GetStatusCodeAsync(result)).Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task MapException_ValidationException_Returns_400()
    {
        var ex = new ValidationException("Validation failed.");
        var result = ExceptionMapping.MapException(ex);
        (await GetStatusCodeAsync(result)).Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task MapException_GenericException_Returns_500()
    {
        var ex = new Exception("Internal error.");
        var result = ExceptionMapping.MapException(ex);
        (await GetStatusCodeAsync(result)).Should().Be(StatusCodes.Status500InternalServerError);
    }
}