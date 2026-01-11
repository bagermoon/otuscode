using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NodaMoney;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Infrastructure.Configuration;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
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

        builder.OwnsOne(r => r.Address, e =>
        {
            e.Property(em => em.FullAddress);
            e.Property(em => em.House);
        });

        builder.OwnsOne(r => r.Location, l =>
        {
            l.Property(loc => loc.Latitude);
            l.Property(loc => loc.Longitude);
        });

        builder.OwnsOne(r => r.OpenHours, l =>
        {
            l.Property(loc => loc.DayOfWeek);
            l.Property(loc => loc.OpenTime);
            l.Property(loc => loc.CloseTime);
            l.Property(loc => loc.IsClosed);
        });

        builder.ComplexProperty(r => r.AverageCheck, ac =>
        {
            ac.Property(m => m.Amount)
                .HasColumnName("average_check_amount")
                .HasPrecision(18, 2);

            ac.Property(m => m.Currency)
                .HasConversion(
                    c => c.Code,
                    code => Currency.FromCode(code))
                .HasColumnName("average_check_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(r => r.RestaurantStatus)
            .HasConversion(
                p => p.Value,
                p => RestaurantStatus.FromValue(p));

        builder.HasMany(r => r.CuisineTypes)
            .WithOne()
            .HasForeignKey(ct => ct.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Tags)
            .WithOne()
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Images)
            .WithOne()
            .HasForeignKey(i => i.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("restaurants");
    }
}
