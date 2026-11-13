using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uber.DAL.Entities;
namespace Uber.Infrastructure.Persistence.Configurations;
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(user => user.RowVersion)
            .IsRowVersion();
    }
}
