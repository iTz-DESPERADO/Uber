using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Drivers.GetDriverProfile;

public sealed record GetDriverProfileQuery(string DriverId)
    : IQuery<DriverProfileResponse>;
