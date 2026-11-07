using Uber.Domain.Common;
using Uber.Domain.Enums;
using Uber.Domain.Rides;
using Xunit;

namespace Uber.Domain.Tests;

public sealed class RideTransitionRulesTests
{
    [Theory]
    [InlineData("arrival", RideStatus.Accepted, "Ride.CannotMarkDriverWaiting", "Cannot mark driver as arrived")]
    [InlineData("start", RideStatus.DriverWaiting, "Ride.CannotStart", "Cannot start ride")]
    [InlineData("complete", RideStatus.InProgress, "Ride.CannotComplete", "Cannot complete ride")]
    public void Rules_preserve_the_allowed_transition_and_expose_stable_error_codes(
        string action, RideStatus expected, string code, string prefix)
    {
        Func<RideStatus, Result> rule = action switch
        {
            "arrival" => RideTransitionRules.CanMarkDriverWaiting,
            "start" => RideTransitionRules.CanStart,
            "complete" => RideTransitionRules.CanComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        foreach (var current in Enum.GetValues<RideStatus>().Append((RideStatus)99))
        {
            var result = rule(current);
            Assert.Equal(current == expected, result.IsSuccess);
            if (result.IsSuccess)
            {
                Assert.Null(result.Error);
            }
            else
            {
                Assert.Equal(code, result.Error!.Code);
                Assert.Equal($"{prefix}. Ride is currently in {current} status. Expected: {expected}", result.Error.Message);
            }
        }
    }

    [Fact]
    public void Enum_names_and_numeric_values_preserve_storage_and_binding_contracts()
    {
        AssertEnum<Gender>(["Male", "Female"]);
        AssertEnum<PaymentMethod>(["Wallet", "Cash"]);
        AssertEnum<RideStatus>(["Pending", "Accepted", "Rejected", "Cancelled", "DriverWaiting", "InProgress", "Completed"]);
        AssertEnum<Transmission>(["Manual", "Automatic"]);
        AssertEnum<VehicleEngine>(["Gasoline", "Electric"]);
        AssertEnum<VehicleType>(["Car", "Scooter", "Shuttle"]);
    }

    private static void AssertEnum<T>(string[] names) where T : struct, Enum
    {
        Assert.Equal(names, Enum.GetNames<T>());
        Assert.Equal(Enumerable.Range(0, names.Length), Enum.GetValues<T>().Select(value => Convert.ToInt32(value)));
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(T)));
    }

    [Fact]
    public void Domain_does_not_reference_web_persistence_or_outer_layers()
    {
        var references = typeof(RideTransitionRules).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, reference =>
            reference.Name!.StartsWith("Uber.") ||
            reference.Name.StartsWith("Microsoft.AspNetCore") ||
            reference.Name.StartsWith("Microsoft.EntityFrameworkCore") ||
            reference.Name.StartsWith("Stripe"));
    }
}
