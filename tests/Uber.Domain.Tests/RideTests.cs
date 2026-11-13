using Uber.Domain.Entities;
using Uber.Domain.Enums;
using Xunit;

namespace Uber.Domain.Tests;

public sealed class RideTests
{
    [Fact]
    public void Ride_follows_the_happy_path_transitions()
    {
        var ride = CreateRide();

        Assert.True(ride.Accept().IsSuccess);
        Assert.True(ride.MarkDriverArrived().IsSuccess);
        Assert.True(ride.Start().IsSuccess);
        Assert.True(ride.Complete().IsSuccess);
        Assert.Equal(RideStatus.Completed, ride.Status);
    }

    [Fact]
    public void Ride_cannot_start_before_driver_arrives()
    {
        var ride = CreateRide();
        ride.Accept();

        var result = ride.Start();

        Assert.True(result.IsFailure);
        Assert.Equal("Ride.CannotStart", result.Error!.Code);
        Assert.Equal(RideStatus.Accepted, ride.Status);
    }

    private static Ride CreateRide()
    {
        var result = Ride.Create(
            "customer-1",
            "driver-1",
            30.0,
            31.0,
            30.1,
            31.1,
            5,
            12,
            100,
            PaymentMethod.Wallet);

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
