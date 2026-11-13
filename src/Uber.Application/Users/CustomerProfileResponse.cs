namespace Uber.Application.Users;

public sealed record CustomerProfileResponse(
    CustomerResponse Customer,
    IReadOnlyList<CustomerRideResponse> Rides);
