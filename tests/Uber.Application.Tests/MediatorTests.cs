using Microsoft.Extensions.DependencyInjection;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;
using Xunit;
using static Uber.Application.Tests.HandlerRegistrationTests;

namespace Uber.Application.Tests;

public sealed class MediatorTests
{
    private static ServiceProvider CreateProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddApplication(typeof(MediatorTests).Assembly).AddApplication(typeof(MediatorTests).Assembly);
        services.AddScoped<Probe>();
        services.AddScoped<Trace>();
        configure?.Invoke(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [Fact]
    public async Task Send_dispatches_all_request_shapes_and_preserves_results()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        Assert.True((await mediator.Send(new Save())).IsSuccess);
        Assert.Equal(42, (await mediator.Send(new Create(42))).Value);
        IRequest<Result<string>> query = new Find(true);
        Assert.Equal("existing", (await mediator.Send(query)).Value);
        Assert.Same(FindHandler.NotFound, (await mediator.Send(new Find(false))).Error);
    }

    [Fact]
    public async Task Open_and_closed_behaviors_wrap_handler_in_registration_order()
    {
        using var provider = CreateProvider(services =>
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, InnerBehavior>();
        });
        using var scope = provider.CreateScope();
        using var source = new CancellationTokenSource();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new Tracked(), source.Token);
        var trace = scope.ServiceProvider.GetRequiredService<Trace>();

        Assert.Equal(7, result.Value);
        Assert.Equal(new[] { "outer-before", "inner-before", "handler", "inner-after", "outer-after" }, trace.Events);
        Assert.Equal(3, trace.Tokens.Count);
        Assert.All(trace.Tokens, token => Assert.Equal(source.Token, token));
    }

    [Fact]
    public async Task Behavior_can_short_circuit_without_constructing_handler()
    {
        using var provider = CreateProvider(services =>
        {
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, StopBehavior>();
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, InnerBehavior>();
        });
        using var scope = provider.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new Tracked());
        var trace = scope.ServiceProvider.GetRequiredService<Trace>();

        Assert.Same(StopBehavior.Rejected, result.Error);
        Assert.Equal(new[] { "stop" }, trace.Events);
        Assert.Equal(0, trace.HandlerInstances);
    }

    [Fact]
    public async Task Mediator_handlers_and_behaviors_use_the_current_scope()
    {
        using var provider = CreateProvider(services =>
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, InnerBehavior>());
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var firstMediator = first.ServiceProvider.GetRequiredService<IMediator>();
        var secondMediator = second.ServiceProvider.GetRequiredService<IMediator>();

        Assert.Single(first.ServiceProvider.GetServices<IMediator>());
        Assert.Same(firstMediator, first.ServiceProvider.GetRequiredService<IMediator>());
        Assert.NotSame(firstMediator, secondMediator);
        await firstMediator.Send(new Tracked());
        await firstMediator.Send(new Tracked());
        await secondMediator.Send(new Tracked());

        var firstTrace = first.ServiceProvider.GetRequiredService<Trace>();
        var secondTrace = second.ServiceProvider.GetRequiredService<Trace>();
        Assert.Equal(1, firstTrace.HandlerInstances);
        Assert.Equal(1, secondTrace.HandlerInstances);
        Assert.Equal(6, firstTrace.Events.Count);
        Assert.Equal(3, secondTrace.Events.Count);
    }

    [Fact]
    public async Task Already_cancelled_request_does_not_run_pipeline_or_handler()
    {
        using var provider = CreateProvider(services =>
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, InnerBehavior>());
        using var scope = provider.CreateScope();
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.ServiceProvider.GetRequiredService<IMediator>().Send(new Tracked(), source.Token));
        var trace = scope.ServiceProvider.GetRequiredService<Trace>();
        Assert.Empty(trace.Events);
        Assert.Equal(0, trace.HandlerInstances);
    }

    [Fact]
    public async Task Cancellation_during_pipeline_prevents_handler_execution()
    {
        using var source = new CancellationTokenSource();
        using var provider = CreateProvider(services =>
        {
            services.AddSingleton(source);
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, CancelBehavior>();
        });
        using var scope = provider.CreateScope();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.ServiceProvider.GetRequiredService<IMediator>().Send(new Tracked(), source.Token));
        Assert.Equal(0, scope.ServiceProvider.GetRequiredService<Trace>().HandlerInstances);
    }

    [Fact]
    public async Task Handler_exception_is_unwrapped_and_outer_behavior_can_observe_it()
    {
        using var provider = CreateProvider(services =>
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>)));
        using var scope = provider.CreateScope();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scope.ServiceProvider.GetRequiredService<IMediator>().Send(new Tracked(Throw: true)));

        Assert.Same(TrackedHandler.Failure, exception);
        Assert.Equal(new[] { "outer-before", "handler", "outer-error" },
            scope.ServiceProvider.GetRequiredService<Trace>().Events);
    }

    [Fact]
    public async Task Pipeline_exception_propagates_without_running_handler()
    {
        using var provider = CreateProvider(services =>
            services.AddScoped<IPipelineBehavior<Tracked, Result<int>>, ThrowBehavior>());
        using var scope = provider.CreateScope();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scope.ServiceProvider.GetRequiredService<IMediator>().Send(new Tracked()));

        Assert.Same(ThrowBehavior.Failure, exception);
        Assert.Equal(0, scope.ServiceProvider.GetRequiredService<Trace>().HandlerInstances);
    }

    [Fact]
    public async Task Missing_handler_and_null_request_fail_explicitly()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send<Result>(null!));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new Unregistered()));
        Assert.Contains(nameof(Unregistered), error.Message);
    }

    public sealed record Tracked(bool Throw = false) : ICommand<int>;
    public sealed record Unregistered : ICommand;

    public sealed class Trace
    {
        public List<string> Events { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];
        public int HandlerInstances { get; set; }
    }

    public sealed class TrackedHandler : ICommandHandler<Tracked, int>
    {
        public static readonly InvalidOperationException Failure = new("Handler failed");
        private readonly Trace _trace;

        public TrackedHandler(Trace trace)
        {
            _trace = trace;
            trace.HandlerInstances++;
        }

        public async Task<Result<int>> HandleAsync(Tracked request, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            _trace.Events.Add("handler");
            _trace.Tokens.Add(cancellationToken);
            if (request.Throw) throw Failure;
            return Result<int>.Success(7);
        }
    }

    public sealed class OuterBehavior<TRequest, TResponse>(Trace trace) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            trace.Events.Add("outer-before");
            trace.Tokens.Add(cancellationToken);
            try
            {
                var result = await next();
                trace.Events.Add("outer-after");
                return result;
            }
            catch
            {
                trace.Events.Add("outer-error");
                throw;
            }
        }
    }

    public sealed class InnerBehavior(Trace trace) : IPipelineBehavior<Tracked, Result<int>>
    {
        public async Task<Result<int>> HandleAsync(Tracked request, RequestHandlerDelegate<Result<int>> next,
            CancellationToken cancellationToken = default)
        {
            trace.Events.Add("inner-before");
            trace.Tokens.Add(cancellationToken);
            var result = await next();
            trace.Events.Add("inner-after");
            return result;
        }
    }

    public sealed class StopBehavior(Trace trace) : IPipelineBehavior<Tracked, Result<int>>
    {
        public static readonly Error Rejected = new("Example.Rejected", "Request rejected");

        public Task<Result<int>> HandleAsync(Tracked request, RequestHandlerDelegate<Result<int>> next,
            CancellationToken cancellationToken = default)
        {
            trace.Events.Add("stop");
            return Task.FromResult(Result<int>.Failure(Rejected));
        }
    }

    public sealed class CancelBehavior(CancellationTokenSource source) : IPipelineBehavior<Tracked, Result<int>>
    {
        public Task<Result<int>> HandleAsync(Tracked request, RequestHandlerDelegate<Result<int>> next,
            CancellationToken cancellationToken = default)
        {
            source.Cancel();
            return next();
        }
    }

    public sealed class ThrowBehavior : IPipelineBehavior<Tracked, Result<int>>
    {
        public static readonly InvalidOperationException Failure = new("Behavior failed");

        public Task<Result<int>> HandleAsync(Tracked request, RequestHandlerDelegate<Result<int>> next,
            CancellationToken cancellationToken = default) => throw Failure;
    }
}
