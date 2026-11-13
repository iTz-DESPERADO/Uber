using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Admins.GetAdmin;

public sealed record GetAdminQuery(string AdminId) : IQuery<AdminResponse>;
