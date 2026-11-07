using Microsoft.Extensions.DependencyInjection;
using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Messaging;

internal sealed class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        // Only the stateless adapter uses reflection. Handlers and behaviors are
        // resolved from the caller's scope, never cached or invoked by reflection.
        var executorType = typeof(RequestExecutor<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var executor = (RequestExecutor<TResponse>)Activator.CreateInstance(executorType)!;
        return executor.Execute(request, services, cancellationToken);
    }
}

internal abstract class RequestExecutor<TResponse>
{
    public abstract Task<TResponse> Execute(
        IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestExecutor<TRequest, TResponse> : RequestExecutor<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Execute(
        IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        RequestHandlerDelegate<TResponse> next = () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return services.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                .HandleAsync(typedRequest, cancellationToken);
        };

        // The first registered behavior is the outermost behavior.
        foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var continuation = next;
            next = () => behavior.HandleAsync(typedRequest, continuation, cancellationToken);
        }

        return next();
    }
}
