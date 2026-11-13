using Uber.Application.Abstractions.Messaging;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;
using Uber.Domain.Repositories;

namespace Uber.Application.Admins.UpdateAdmin;

public sealed class UpdateAdminHandler(
    IAdminRepository admins,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAdminCommand>
{
    public async Task<Result> HandleAsync(
        UpdateAdminCommand request,
        CancellationToken cancellationToken = default)
    {
        var admin = await admins.GetByIdAsync(request.AdminId, cancellationToken);

        if (admin is null)
        {
            return Result.Failure(new Error("Admin.NotFound", "Admin not found"));
        }

        var result = admin.Edit(
            request.Name,
            request.DateOfBirth,
            request.Email,
            request.PhoneNumber,
            request.IsDeleted);

        if (result.IsFailure)
        {
            return result;
        }

        await admins.UpdateAsync(admin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
