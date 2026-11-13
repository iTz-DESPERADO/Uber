using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Admins.DeleteAdmin;

public sealed class DeleteAdminHandler(
    IAdminRepository admins,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAdminCommand>
{
    public async Task<Result> HandleAsync(
        DeleteAdminCommand request,
        CancellationToken cancellationToken = default)
    {
        var admin = await admins.GetByIdAsync(request.AdminId, cancellationToken);

        if (admin is null)
        {
            return Result.Failure(new Error("Admin.NotFound", "Admin not found"));
        }

        var result = admin.Delete();

        if (result.IsFailure)
        {
            return result;
        }

        await admins.UpdateAsync(admin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
