using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NodaMoney;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;

namespace RestoRate.RestaurantService.Infrastructure.Configuration;

public class RestaurantRatingConfiguration : IEntityTypeConfiguration<RatingSnapshot>
{
    public void Configure(EntityTypeBuilder<RatingSnapshot> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RestaurantId)
            .IsRequired();

        builder.Property(r => r.AverageRate)
            .HasColumnName("approved_average_rating")
            .HasPrecision(18, 2);

        builder.Property(r => r.ReviewCount)
            .HasColumnName("approved_reviews_count");

        builder.ComplexProperty(r => r.AverageCheck, pr =>
        {
            pr.Property(m => m.Amount)
                .HasColumnName("approved_average_check_amount")
                .HasPrecision(18, 2);

            pr.Property(m => m.Currency)
                .HasColumnName("approved_average_check_currency")
                .HasConversion(
                    c => c.Code,
                    code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Property(r => r.ProvisionalAverageRate)
            .HasColumnName("provisional_average_rating")
            .HasPrecision(18, 2);

        builder.Property(r => r.ProvisionalReviewCount)
            .HasColumnName("provisional_reviews_count");

        builder.ComplexProperty(r => r.ProvisionalAverageCheck, ac =>
        {
            ac.Property(m => m.Amount)
                .HasColumnName("provisional_average_check_amount")
                .HasPrecision(18, 2);

            ac.Property(m => m.Currency)
                .HasColumnName("provisional_average_check_currency")
                .HasConversion(
                    c => c.Code,
                    code => Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(r => r.RestaurantId).IsUnique();
        builder.HasIndex(r => r.AverageRate);

        builder.HasOne<Restaurant>()
            .WithOne(r => r.Rating)
            .HasForeignKey<RatingSnapshot>(r => r.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("restaurant_ratings");
    }
}
