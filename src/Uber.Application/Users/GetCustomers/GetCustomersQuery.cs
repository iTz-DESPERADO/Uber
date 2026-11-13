using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Users.GetCustomers;

public sealed record GetCustomersQuery : IQuery<IReadOnlyList<CustomerResponse>>;
