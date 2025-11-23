using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestoRate.Restaurant.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.Restaurant.Infrastructure.Configuration;

public class RestaurantTagConfiguration : IEntityTypeConfiguration<RestaurantTag>
{
    public void Configure(EntityTypeBuilder<RestaurantTag> builder)
    {
        builder.ToTable("restaurant_tags");

        builder.HasKey(i => i.Id);

        builder.Property(r => r.Tag)
            .HasConversion(
                r => r.Value,
                value => Tag.FromValue(value)
            );
    }
}
