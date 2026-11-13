namespace Uber.Domain.Abstractions;

using Uber.Domain.Common;

public interface IUnitOfWork
{
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Result> ExecuteInTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default);
}
