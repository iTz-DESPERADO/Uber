using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Uber.Application;
using Uber.Infrastructure;
using Uber.DAL.DataBase;
using Uber.DAL.Entities;
using Uber.Ui.Authorization.Permissions;
using Uber.Ui.Endpoints.Rides;
using Uber.Ui.ExceptionHandling;
using Uber.Ui.Endpoints.Admins;
using Uber.Ui.Endpoints.Drivers;
using Uber.Ui.Endpoints.Users;
using Uber.Ui.Endpoints.Payments;
using Uber.Ui.Endpoints.Authentication;
using Uber.Infrastructure.Identity;
using Uber.Ui.Endpoints.Locations;
using System.Text.Json.Serialization;

namespace Uber.Ui
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            var connectionString = builder.Configuration.GetConnectionString("Mydatabase")
                ?? throw new InvalidOperationException(
                    "Connection string 'Mydatabase' is not configured.");

            builder.Services.AddDbContext<UberDBContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
            })
                .AddEntityFrameworkStores<UberDBContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
            });

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

            builder.Services.AddHttpContextAccessor();

            var externalAuthentication = builder.Services.AddAuthentication();
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

            if (!string.IsNullOrWhiteSpace(googleClientId)
                && !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                externalAuthentication.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.SaveTokens = true;
                    options.AccessDeniedPath = "/Account/Login";
                });
            }

            var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

            if (!string.IsNullOrWhiteSpace(facebookAppId)
                && !string.IsNullOrWhiteSpace(facebookAppSecret))
            {
                externalAuthentication.AddFacebook(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                    options.SaveTokens = true;
                    options.AccessDeniedPath = "/Account/Login";
                });
            }

            var app = builder.Build();
            await app.Services.InitializeIdentityAsync();

            if (app.Environment.IsDevelopment())
            {
                await app.Services.SeedDevelopmentDataAsync();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapHub<RideHub>("/rideHub");
            app.MapRideQueryEndpoints();
            app.MapRideCommandEndpoints();
            app.MapUserEndpoints();
            app.MapDriverEndpoints();
            app.MapAdminEndpoints();
            app.MapPaymentEndpoints();
            app.MapRegistrationEndpoints();
            app.MapLocationEndpoints();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
