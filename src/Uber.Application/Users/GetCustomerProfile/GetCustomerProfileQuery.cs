using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Users.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(string CustomerId)
    : IQuery<CustomerProfileResponse>;
