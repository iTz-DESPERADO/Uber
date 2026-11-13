using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Uber.Application;
using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Common;
using Xunit;

namespace Uber.Application.Tests;

public sealed class MediatorPipelineTests
{
    [Fact]
    public async Task Valid_request_reaches_handler_and_preserves_result()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IMediator>()
            .Send(new TestCommand("ride-42"));

        Assert.True(result.IsSuccess);
        Assert.Equal("ride-42", result.Value);
        Assert.Equal(1, scope.ServiceProvider.GetRequiredService<HandlerProbe>().Calls);
    }

    [Fact]
    public async Task Invalid_request_is_stopped_by_validation_pipeline()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        var result = await scope.ServiceProvider
            .GetRequiredService<IMediator>()
            .Send(new TestCommand(string.Empty));

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Failed", result.Error!.Code);
        Assert.Equal(0, scope.ServiceProvider.GetRequiredService<HandlerProbe>().Calls);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddApplication(typeof(MediatorPipelineTests).Assembly);
        services.AddScoped<HandlerProbe>();
        services.AddScoped<IValidator<TestCommand>, TestCommandValidator>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
    }

    public sealed record TestCommand(string Value) : ICommand<string>;

    public sealed class TestCommandHandler(HandlerProbe probe) : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> HandleAsync(
            TestCommand command,
            CancellationToken cancellationToken = default)
        {
            probe.Calls++;
            return Task.FromResult(Result<string>.Success(command.Value));
        }
    }

    public sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator() => RuleFor(command => command.Value).NotEmpty();
    }

    public sealed class HandlerProbe
    {
        public int Calls { get; set; }
    }
}
