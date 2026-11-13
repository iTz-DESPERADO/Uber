using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uber.DAL.Entities;
using Uber.Domain.Abstractions;
using Uber.Domain.Common;

namespace Uber.DAL.DataBase;

public class UberDBContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    private readonly List<Action> _generatedIdSynchronizers = [];

    public UberDBContext(DbContextOptions<UberDBContext> options)
        : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public new DbSet<User> Users { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<RideRating> RideRatings { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    public void SynchronizeGeneratedIdAfterSave(Action synchronize)
    {
        ArgumentNullException.ThrowIfNull(synchronize);
        _generatedIdSynchronizers.Add(synchronize);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            var count = base.SaveChanges(acceptAllChangesOnSuccess);
            SynchronizeGeneratedIds();
            return count;
        }
        finally
        {
            _generatedIdSynchronizers.Clear();
        }
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            SynchronizeGeneratedIds();
            return count;
        }
        finally
        {
            _generatedIdSynchronizers.Clear();
        }
    }

    public async Task<Result> ExecuteInTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        var result = await operation();

        if (result.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UberDBContext).Assembly);
    }

    private void SynchronizeGeneratedIds()
    {
        foreach (var synchronize in _generatedIdSynchronizers)
        {
            synchronize();
        }
    }
}
