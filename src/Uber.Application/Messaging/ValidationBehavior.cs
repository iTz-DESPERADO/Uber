using System.Reflection;
using FluentValidation;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Abstractions.Validation;
using Uber.Domain.Common;

namespace Uber.Application.Messaging;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        return ResultFactory<TResponse>.Failure(ValidationErrors.From(failures));
    }

    private static class ResultFactory<TResult>
        where TResult : Result
    {
        private static readonly Func<Error, TResult> Create = BuildFactory();

        public static TResult Failure(Error error) => Create(error);

        private static Func<Error, TResult> BuildFactory()
        {
            var method = typeof(TResult).GetMethod(
                nameof(Result.Failure),
                BindingFlags.Public | BindingFlags.Static,
                [typeof(Error)]);

            if (method is null)
            {
                throw new InvalidOperationException(
                    $"Response type '{typeof(TResult).Name}' does not expose Failure(Error).");
            }

            return error => (TResult)method.Invoke(null, [error])!;
        }
    }
}
