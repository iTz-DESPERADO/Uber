using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Uber.Domain.Repositories;
using Uber.Domain.Abstractions;
using Uber.DAL.DataBase;
using Uber.Infrastructure.Persistence;
using Uber.Application.Abstractions.Data;
using Uber.Infrastructure.Data;
using Uber.Application.Abstractions.Payments;
using Uber.Infrastructure.Payments;
using Uber.Application.Abstractions.Identity;
using Uber.Infrastructure.Identity;
using Uber.Application.Abstractions.Email;
using Uber.Infrastructure.Email;
using Uber.Application.Abstractions.Locations;
using Uber.Infrastructure.Locations;

namespace Uber.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.TryAddScoped<IRideRepository, RideRepository>();

        services.TryAddScoped<IRideRatingRepository, RideRatingRepository>();

        services.TryAddScoped<IUserRepository, UserRepository>();

        services.TryAddScoped<IDriverRepository, DriverRepository>();

        services.TryAddScoped<IAdminRepository, AdminRepository>();

        services.TryAddScoped<IVehicleRepository, VehicleRepository>();

        services.TryAddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UberDBContext>());

        services.TryAddScoped<IDbConnectionFactory, SqlConnectionFactory>();

        services.TryAddScoped<IPaymentGateway, StripePaymentGateway>();

        services.TryAddScoped<IIdentityRegistrationService, IdentityRegistrationService>();

        services.TryAddScoped<IEmailService, SmtpEmailService>();

        services.AddHttpClient<ILocationSearchService, NominatimLocationSearchService>(client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("UberRideApp/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
