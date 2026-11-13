using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Uber.DAL.Entities;

namespace Uber.Infrastructure.Persistence.Configurations;

internal sealed class RideRatingConfiguration : IEntityTypeConfiguration<RideRating>
{
    public void Configure(EntityTypeBuilder<RideRating> builder)
    {
        builder.HasKey(rating => rating.Id);
        builder.Property(rating => rating.Id).ValueGeneratedOnAdd();

        builder.Property(rating => rating.RaterId).IsRequired();
        builder.Property(rating => rating.RatedUserId).IsRequired();
        builder.Property(rating => rating.CreatedAt).IsRequired();

        builder.HasIndex(rating => new { rating.RideId, rating.RaterId })
            .IsUnique();

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_RideRatings_Value",
            "[Value] >= 1 AND [Value] <= 5"));

        builder.HasOne(rating => rating.Ride)
            .WithMany()
            .HasForeignKey(rating => rating.RideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rating => rating.Rater)
            .WithMany()
            .HasForeignKey(rating => rating.RaterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rating => rating.RatedUser)
            .WithMany()
            .HasForeignKey(rating => rating.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
