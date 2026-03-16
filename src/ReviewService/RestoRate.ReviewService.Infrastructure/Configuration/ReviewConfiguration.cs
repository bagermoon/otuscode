using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Infrastructure.Configuration;

internal sealed class ReviewConfiguration() : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToCollection("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RestaurantId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Ignore(r => r.AverageCheck);

        builder.OwnsOne<ReviewAverageCheck>("AverageCheckData", averageCheck =>
        {
            averageCheck.HasElementName("AverageCheck");

            averageCheck.Property(m => m.Amount)
                .IsRequired();

            averageCheck.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Navigation("AverageCheckData")
            .IsRequired(false);

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.RejectionSource)
            .IsRequired();

        builder.HasOne(r => r.Restaurant)
            .WithMany()
            .HasForeignKey(r => r.RestaurantId)
            .IsRequired(false);

        builder.Navigation(rt => rt.Restaurant)
            .AutoInclude(false); // добавлен в спецификации

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .IsRequired(false);

        builder.Navigation(rt => rt.User)
            .AutoInclude(false); // добавлен в спецификации
    }
}
