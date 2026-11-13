using Uber.Application.Abstractions.Messaging;

namespace Uber.Application.Admins.DeleteAdmin;

public sealed record DeleteAdminCommand(string AdminId) : ICommand;
