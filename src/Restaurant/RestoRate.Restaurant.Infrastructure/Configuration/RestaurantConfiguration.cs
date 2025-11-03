using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            pn.Property(p => p.OperatorCode).HasColumnName("PhoneNumber_OperatorCode");
            pn.Property(p => p.Number).HasColumnName("PhoneNumber_Number");
            pn.Property(p => p.Extension).HasColumnName("PhoneNumber_Extension");
        });

        builder.OwnsOne(r => r.Email, e =>
        {
            e.Property(em => em.Address).HasColumnName("Email_Address");
        });

        builder.OwnsOne(r => r.Location, l =>
        {
            l.Property(loc => loc.Latitude).HasColumnName("Location_Latitude");
            l.Property(loc => loc.Longitude).HasColumnName("Location_Longitude");
        });

        builder.OwnsOne(r => r.AverageCheck, ac =>
        {
            ac.Property(m => m.Amount).HasColumnName("AverageCheck_Amount");
            ac.Property(m => m.Currency).HasColumnName("AverageCheck_Currency");
        });

        builder.Property(r => r.Tag)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.ToTable("Restaurants");
    }
}
