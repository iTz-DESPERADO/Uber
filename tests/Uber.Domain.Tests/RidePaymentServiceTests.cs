using Uber.Domain.Entities;
using Uber.Domain.Enums;
using Uber.Domain.Services;
using Xunit;

namespace Uber.Domain.Tests;

public sealed class RidePaymentServiceTests
{
    [Fact]
    public void Wallet_payment_debits_customer_and_credits_driver_share()
    {
        var customer = CreateCustomer("customer-1");
        customer.AddBalance(150);
        var driver = CreateDriver("driver-1");
        var ride = CreateRide(customer.Id, driver.Id);

        var result = new RidePaymentService().SettleCompletedRide(ride, customer, driver);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, customer.Balance);
        Assert.Equal(80, driver.Balance);
    }

    [Fact]
    public void Mismatched_customer_does_not_change_balances()
    {
        var customer = CreateCustomer("another-customer");
        customer.AddBalance(150);
        var driver = CreateDriver("driver-1");
        var ride = CreateRide("customer-1", driver.Id);

        var result = new RidePaymentService().SettleCompletedRide(ride, customer, driver);

        Assert.True(result.IsFailure);
        Assert.Equal(150, customer.Balance);
        Assert.Equal(0, driver.Balance);
    }

    private static Customer CreateCustomer(string id) => Customer.Create(
        id,
        "Customer",
        Gender.Male,
        new DateTime(1995, 1, 1),
        "customer@example.com",
        null).Value;

    private static Driver CreateDriver(string id) => Driver.Create(
        id,
        "Driver",
        Gender.Male,
        new DateTime(1990, 1, 1),
        "driver@example.com",
        null,
        1).Value;

    private static Ride CreateRide(string customerId, string driverId)
    {
        var ride = Ride.Create(
            customerId,
            driverId,
            30.0,
            31.0,
            30.1,
            31.1,
            5,
            12,
            100,
            PaymentMethod.Wallet).Value;

        ride.Accept();
        ride.MarkDriverArrived();
        ride.Start();
        ride.Complete();

        return ride;
    }
}
