using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uber.DAL.Entities;
namespace Uber.Infrastructure.Persistence.Configurations;
internal sealed class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.HasKey(ride => ride.Id);
        builder.Property(ride => ride.Id).ValueGeneratedOnAdd();
        builder.Property(ride => ride.RowVersion).IsRowVersion();

        builder.HasIndex(ride => ride.DriverId)
            .IsUnique()
            .HasDatabaseName("UX_Rides_ActiveDriver")
            .HasFilter("[DriverId] IS NOT NULL AND [Status] IN (1, 4, 5)");

        builder.HasOne(ride => ride.Driver).WithMany(driver => driver.Rides)
            .HasForeignKey(ride => ride.DriverId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(ride => ride.User).WithMany(user => user.Rides)
            .HasForeignKey(ride => ride.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
