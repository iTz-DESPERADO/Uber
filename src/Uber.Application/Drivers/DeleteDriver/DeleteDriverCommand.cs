using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.DeleteDriver;

public sealed record DeleteDriverCommand(string DriverId) : ICommand;
