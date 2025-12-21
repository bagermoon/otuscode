using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RestoRate.RestaurantService.Domain.TagAggregate;

namespace RestoRate.RestaurantService.Infrastructure.Configuration;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .HasIndex(t => t.NormalizedName).IsUnique();
    }
}
