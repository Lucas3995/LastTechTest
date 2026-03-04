using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Aplicacao.Common.Behaviors;

/// <summary>
/// RC-1: Pipeline behavior that runs FluentValidation for any request that has a registered validator.
/// When validation fails, throws <see cref="ValidationException"/> so the API can return 400.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>();
        if (validator is null)
            return await next();

        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
            return await next();

        throw new ValidationException(result.Errors);
    }
}