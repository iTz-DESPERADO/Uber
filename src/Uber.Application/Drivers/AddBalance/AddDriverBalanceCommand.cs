using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.AddBalance;

public sealed record AddDriverBalanceCommand(
    string DriverId,
    double Amount) : ICommand;
