using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Admins.GetAdmins;

public sealed record GetAdminsQuery : IQuery<IReadOnlyList<AdminResponse>>;
