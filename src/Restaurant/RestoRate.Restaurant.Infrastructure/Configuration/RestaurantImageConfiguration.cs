using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoRate.Restaurant.Domain.RestaurantAggregate;

namespace RestoRate.Restaurant.Infrastructure.Configuration;

public class RestaurantImageConfiguration : IEntityTypeConfiguration<RestaurantImage>
{
    public void Configure(EntityTypeBuilder<RestaurantImage> builder)
    {
        builder.ToTable("restaurant_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.AltText)
            .HasMaxLength(200);

        builder.Property(i => i.DisplayOrder)
            .IsRequired();

        builder.Property(i => i.IsPrimary)
            .IsRequired();

        builder.HasIndex(i => new { i.RestaurantId, i.IsPrimary });
    }
}
