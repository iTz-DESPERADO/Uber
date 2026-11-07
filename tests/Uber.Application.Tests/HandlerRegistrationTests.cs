using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Reflection.Emit;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;
using Xunit;

namespace Uber.Application.Tests;

public sealed class HandlerRegistrationTests
{
    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped<Probe>();
        services.AddApplication(typeof(HandlerRegistrationTests).Assembly);
        services.AddScoped<MediatorTests.Trace>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [Fact]
    public async Task All_handler_shapes_resolve_with_dependencies_and_preserve_results()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var command = scope.ServiceProvider.GetRequiredService<IRequestHandler<Save, Result>>();
        var create = scope.ServiceProvider.GetRequiredService<IRequestHandler<Create, Result<int>>>();
        var query = scope.ServiceProvider.GetRequiredService<IRequestHandler<Find, Result<string>>>();

        Assert.True((await command.HandleAsync(new Save())).IsSuccess);
        Assert.Equal(42, (await create.HandleAsync(new Create(42))).Value);
        Assert.Equal("existing", (await query.HandleAsync(new Find(true))).Value);

        var failure = await query.HandleAsync(new Find(false));
        Assert.True(failure.IsFailure);
        Assert.Same(FindHandler.NotFound, failure.Error);
    }

    [Fact]
    public void Handlers_are_reused_within_a_scope_and_isolated_between_scopes()
    {
        using var provider = CreateProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        AssertScoped<IRequestHandler<Save, Result>>(first, second);
        AssertScoped<IRequestHandler<Create, Result<int>>>(first, second);
        AssertScoped<IRequestHandler<Find, Result<string>>>(first, second);
    }

    private static void AssertScoped<T>(IServiceScope first, IServiceScope second) where T : class
    {
        var handler = first.ServiceProvider.GetRequiredService<T>();
        Assert.Same(handler, first.ServiceProvider.GetRequiredService<T>());
        Assert.NotSame(handler, second.ServiceProvider.GetRequiredService<T>());
    }

    [Fact]
    public void Repeated_registration_does_not_duplicate_handlers()
    {
        var services = new ServiceCollection();
        services.AddApplication(typeof(HandlerRegistrationTests).Assembly);
        var count = services.Count;
        services.AddApplication(typeof(HandlerRegistrationTests).Assembly, typeof(HandlerRegistrationTests).Assembly);

        Assert.Equal(count, services.Count);
        Assert.Single(services, service => service.ServiceType == typeof(IRequestHandler<Save, Result>));
        Assert.All(services, descriptor => Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime));
    }

    [Fact]
    public void Default_scan_only_registers_application_services()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        Assert.Single(services);
        Assert.Equal(typeof(IMediator), services[0].ServiceType);
    }

    [Fact]
    public void Scan_ignores_abstract_open_generic_and_unrelated_types()
    {
        var services = new ServiceCollection();
        services.AddApplication(typeof(HandlerRegistrationTests).Assembly);

        Assert.DoesNotContain(services, service => service.ImplementationType == typeof(AbstractHandler));
        Assert.DoesNotContain(services, service => service.ImplementationType == typeof(GenericHandler<>));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(Probe));
        Assert.DoesNotContain(services, service => service.ServiceType.IsGenericType &&
            service.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
    }

    [Fact]
    public void Conflicting_existing_registration_is_rejected_without_partial_changes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<Save, Result>, GenericHandler<Save>>();

        var error = Assert.Throws<InvalidOperationException>(() =>
            services.AddApplication(typeof(HandlerRegistrationTests).Assembly));

        Assert.Contains("Conflicting handler", error.Message);
        Assert.Single(services);
    }

    [Fact]
    public void Two_discovered_handlers_for_the_same_request_are_rejected()
    {
        // Isolate conflicting fixtures so the normal test assembly remains valid.
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"DuplicateHandlers_{Guid.NewGuid():N}"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Handlers");
        foreach (var name in new[] { "FirstHandler", "SecondHandler" })
        {
            module.DefineType(name, TypeAttributes.Public | TypeAttributes.Class,
                typeof(GenericHandler<Save>)).CreateType();
        }
        var services = new ServiceCollection();

        var error = Assert.Throws<InvalidOperationException>(() => services.AddApplication(assembly));

        Assert.Contains("Multiple handlers", error.Message);
        Assert.Contains("FirstHandler", error.Message);
        Assert.Contains("SecondHandler", error.Message);
        Assert.Empty(services);
    }

    [Fact]
    public async Task Caller_token_reaches_each_handler_dependency()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        using var source = new CancellationTokenSource();
        var probe = scope.ServiceProvider.GetRequiredService<Probe>();

        await scope.ServiceProvider.GetRequiredService<IRequestHandler<Save, Result>>()
            .HandleAsync(new Save(), source.Token);
        await scope.ServiceProvider.GetRequiredService<IRequestHandler<Create, Result<int>>>()
            .HandleAsync(new Create(1), source.Token);
        await scope.ServiceProvider.GetRequiredService<IRequestHandler<Find, Result<string>>>()
            .HandleAsync(new Find(true), source.Token);

        Assert.Equal(3, probe.Tokens.Count);
        Assert.All(probe.Tokens, token => Assert.Equal(source.Token, token));
    }

    [Fact]
    public async Task Cancellation_is_not_converted_to_a_business_failure()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.ServiceProvider.GetRequiredService<IRequestHandler<Save, Result>>()
                .HandleAsync(new Save(), source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.ServiceProvider.GetRequiredService<IRequestHandler<Create, Result<int>>>()
                .HandleAsync(new Create(1), source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            scope.ServiceProvider.GetRequiredService<IRequestHandler<Find, Result<string>>>()
                .HandleAsync(new Find(true), source.Token));
    }

    public sealed record Save : ICommand;
    public sealed record Create(int Id) : ICommand<int>;
    public sealed record Find(bool Exists) : IQuery<string>;

    public abstract class AbstractHandler : ICommandHandler<Save>
    {
        public abstract Task<Result> HandleAsync(Save request, CancellationToken cancellationToken = default);
    }

    public class GenericHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public Task<Result> HandleAsync(TCommand request, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    public sealed class Probe
    {
        public List<CancellationToken> Tokens { get; } = [];

        public Task ExecuteAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Tokens.Add(token);
            return Task.CompletedTask;
        }
    }

    public sealed class SaveHandler(Probe probe) : ICommandHandler<Save>
    {
        public async Task<Result> HandleAsync(Save command, CancellationToken cancellationToken = default)
        {
            await probe.ExecuteAsync(cancellationToken);
            return Result.Success();
        }
    }

    public sealed class CreateHandler(Probe probe) : ICommandHandler<Create, int>
    {
        public async Task<Result<int>> HandleAsync(Create command, CancellationToken cancellationToken = default)
        {
            await probe.ExecuteAsync(cancellationToken);
            return Result<int>.Success(command.Id);
        }
    }

    public sealed class FindHandler(Probe probe) : IQueryHandler<Find, string>
    {
        public static readonly Error NotFound = new("Example.NotFound", "Example not found");

        public async Task<Result<string>> HandleAsync(Find query, CancellationToken cancellationToken = default)
        {
            await probe.ExecuteAsync(cancellationToken);
            return query.Exists ? Result<string>.Success("existing") : Result<string>.Failure(NotFound);
        }
    }
}
