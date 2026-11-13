using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.RateCustomer;

public sealed record RateCustomerCommand(int RideId, int Rating, string DriverId) : ICommand;
