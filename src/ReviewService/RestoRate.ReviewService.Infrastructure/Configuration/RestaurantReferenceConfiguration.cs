using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MongoDB.EntityFrameworkCore.Extensions;

using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Infrastructure.Configuration;

public class RestaurantReferenceConfiguration : IEntityTypeConfiguration<RestaurantReference>
{
    public void Configure(EntityTypeBuilder<RestaurantReference> builder)
    {
        builder.ToCollection("RestaurantReferences");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.RestaurantStatus)
            .IsRequired();

        builder.Property(r => r.RestaurantStatus)
            .IsRequired()
            .HasConversion(
                p => p.Value,
                p => RestaurantStatus.FromValue(p));
    }
}
