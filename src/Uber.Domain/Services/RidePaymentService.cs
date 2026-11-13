using Uber.Domain.Common;
using Uber.Domain.Entities;
using Uber.Domain.Enums;

namespace Uber.Domain.Services;

public sealed class RidePaymentService
{
    public Result SettleCompletedRide(Ride ride, Customer customer, Driver driver)
    {
        ArgumentNullException.ThrowIfNull(ride);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(driver);

        if (ride.Price is null) 
            return Result.Failure(EntityErrors.RidePriceRequired);

        if (ride.UserId != customer.Id) 
            return Result.Failure(EntityErrors.RideCustomerMismatch);

        if (ride.DriverId != driver.Id) 
            return Result.Failure(EntityErrors.RideDriverMismatch);

        if (ride.Status != RideStatus.Completed)
            return Result.Failure(EntityErrors.RideMustBeCompleted);

        if (ride.PaymentMethod == PaymentMethod.Wallet
            && customer.Balance < ride.Price.Value)
        {
            return Result.Failure(EntityErrors.InsufficientBalance);
        }

        var driverAmount = ride.PaymentMethod == PaymentMethod.Wallet
            ? ride.Price.Value * 0.8
            : ride.Price.Value * -0.2;

        var customerAmount = ride.PaymentMethod == PaymentMethod.Wallet ? ride.Price.Value * -1 : 0;

        if (customerAmount != 0)
        {
            var customerResult = customer.AdjustBalance(customerAmount);

            if (customerResult.IsFailure)
                return customerResult;
        }

        var driverResult = driver.AdjustBalance(driverAmount);

        if (driverResult.IsFailure) 
            return driverResult;


        return driverResult;
    }
}
