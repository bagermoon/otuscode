using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Infrastructure.Configuration;

public class RestaurantCuisineTypeConfiguration : IEntityTypeConfiguration<RestaurantCuisineType>
{
    public void Configure(EntityTypeBuilder<RestaurantCuisineType> builder)
    {
        builder.ToTable("restaurant_cuisinetype");

        builder.HasKey(i => i.Id);

        builder.Property(r => r.CuisineType)
            .HasConversion(
                r => r.Value,
                value => CuisineType.FromValue(value)
            );
    }
}
