using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Uber.Application.Abstractions.Messaging;
using Uber.Application.Messaging;
using Uber.Domain.Services;

namespace Uber.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] additionalAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(additionalAssemblies);

        var registrations = new Dictionary<Type, Type>();
        var assemblies = additionalAssemblies.Prepend(typeof(DependencyInjection).Assembly).Distinct();

        foreach (var assembly in assemblies)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            foreach (var implementation in assembly.GetTypes()
                         .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters))
            {
                foreach (var contract in implementation.GetInterfaces()
                             .Where(type => type.IsGenericType &&
                                            type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                {
                    if (registrations.TryGetValue(contract, out var existing) && existing != implementation)
                    {
                        throw new InvalidOperationException(
                            $"Multiple handlers for '{contract}': '{existing}' and '{implementation}'.");
                    }

                    registrations[contract] = implementation;
                }
            }
        }

        foreach (var (contract, implementation) in registrations)
        {
            var existing = services.Where(service => service.ServiceType == contract).ToArray();
            if (existing.Length > 1 || existing.Any(service =>
                    service.IsKeyedService || service.ImplementationType != implementation ||
                    service.Lifetime != ServiceLifetime.Scoped))
            {
                throw new InvalidOperationException($"Conflicting handler registration for '{contract}'.");
            }
        }

        services.TryAddScoped<IMediator, Mediator>();

        services.TryAddSingleton<RidePaymentService>();
        services.TryAddSingleton<RidePricingService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.TryAddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));


        foreach (var (contract, implementation) in registrations)
        {
            services.TryAddScoped(contract, implementation);
        }

        return services;
    }
}
