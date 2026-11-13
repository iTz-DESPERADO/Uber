using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Rides.RateDriver;

public sealed record RateDriverCommand(int RideId, int Rating, string CustomerId) : ICommand;
