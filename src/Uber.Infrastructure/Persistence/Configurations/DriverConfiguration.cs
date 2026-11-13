using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uber.DAL.Entities;
namespace Uber.Infrastructure.Persistence.Configurations;
internal sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasOne(driver => driver.Vehicle).WithMany().HasForeignKey(driver => driver.VehicleId);
    }
}
