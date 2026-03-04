using FluentAssertions;

using FluentValidation;

using LastTechTest.Aplicacao.Common.Behaviors;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidatorRegistered_Should_CallNext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ValidationBehavior<TestRequest, TestResponse>>(sp =>
            new ValidationBehavior<TestRequest, TestResponse>(sp));
        var provider = services.BuildServiceProvider();
        var behavior = provider.GetRequiredService<ValidationBehavior<TestRequest, TestResponse>>();

        var request = new TestRequest("ok");
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = _ => { nextCalled = true; return Task.FromResult(new TestResponse()); };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().NotBeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorValid_Should_CallNext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestRequest>, TestRequestValidator>();
        services.AddSingleton<ValidationBehavior<TestRequest, TestResponse>>(sp =>
            new ValidationBehavior<TestRequest, TestResponse>(sp));
        var provider = services.BuildServiceProvider();
        var behavior = provider.GetRequiredService<ValidationBehavior<TestRequest, TestResponse>>();

        var request = new TestRequest("valid@example.com");
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = _ => { nextCalled = true; return Task.FromResult(new TestResponse()); };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().NotBeNull();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidatorInvalid_Should_ThrowValidationException()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestRequest>, TestRequestValidator>();
        services.AddSingleton<ValidationBehavior<TestRequest, TestResponse>>(sp =>
            new ValidationBehavior<TestRequest, TestResponse>(sp));
        var provider = services.BuildServiceProvider();
        var behavior = provider.GetRequiredService<ValidationBehavior<TestRequest, TestResponse>>();

        var request = new TestRequest("invalid");
        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email*");
    }

    private sealed record TestRequest(string Email) : IRequest<TestResponse>;

    private sealed record TestResponse;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }
}