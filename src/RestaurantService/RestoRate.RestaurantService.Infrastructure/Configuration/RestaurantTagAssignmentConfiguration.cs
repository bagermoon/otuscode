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

public class RestaurantTagConfiguration : IEntityTypeConfiguration<RestaurantTag>
{
    public void Configure(EntityTypeBuilder<RestaurantTag> builder)
    {
        builder.ToTable("restaurant_tags");

        builder.HasKey(i => i.Id);

        builder.HasOne<Domain.RestaurantAggregate.Restaurant>()
            .WithMany(r => r.Tags)
            .HasForeignKey(rt => rt.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.Tag)
            .WithMany()
            .HasForeignKey(rt => rt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => new { rt.RestaurantId, rt.TagId })
            .IsUnique();

        builder.Navigation(rt => rt.Tag).AutoInclude(false); // добавлен в спецификации
    }
}
