using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RestoRate.SharedKernel.Enums;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Infrastructure.Configuration;

public class RestaurantConfiguration : IEntityTypeConfiguration<Domain.RestaurantAggregate.Restaurant>
{
    public void Configure(EntityTypeBuilder<Domain.RestaurantAggregate.Restaurant> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.OwnsOne(r => r.PhoneNumber, pn =>
        {
            pn.Property(p => p.OperatorCode);
            pn.Property(p => p.Number);
            pn.Property(p => p.Extension);
        });

        builder.OwnsOne(r => r.Email, e =>
        {
            e.Property(em => em.Address);
        });

        builder.OwnsOne(r => r.Location, l =>
        {
            l.Property(loc => loc.Latitude);
            l.Property(loc => loc.Longitude);
        });

        builder.OwnsOne(r => r.AverageCheck, ac =>
        {
            ac.Property(m => m.Amount);
            ac.Property(m => m.Currency);
        });

        builder
            .Property(r => r.Tag)
            .HasConversion(
                r => r.Value,
                value => RestaurantTag.FromValue(value)
            );

        builder.ToTable("restaurants");
    }
}
