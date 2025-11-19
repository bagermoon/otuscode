using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace RestoRate.Review.Infrastructure.Configuration;

internal class ReviewConfiguration : IEntityTypeConfiguration<RestoRate.Review.Domain.ReviewAggregate.Review>
{
    public void Configure(EntityTypeBuilder<RestoRate.Review.Domain.ReviewAggregate.Review> builder)
    {
        builder.ToCollection("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RestaurantId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Text)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);
    }
}
