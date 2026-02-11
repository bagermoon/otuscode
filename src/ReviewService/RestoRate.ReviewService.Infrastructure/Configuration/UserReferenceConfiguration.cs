using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MongoDB.EntityFrameworkCore.Extensions;

using RestoRate.ReviewService.Domain.UserReferenceAggregate;

namespace RestoRate.ReviewService.Infrastructure.Configuration;

public class UserReferenceConfiguration : IEntityTypeConfiguration<UserReference>
{
    public void Configure(EntityTypeBuilder<UserReference> builder)
    {
        builder.ToCollection("UserReferences");
        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Name)
            .HasMaxLength(32);

        builder.Property(rr => rr.FullName)
            .HasMaxLength(200);

        builder.OwnsOne(r => r.Email, e =>
        {
            e.Property(em => em.Address)
                .HasMaxLength(320);
        });

        builder.HasIndex(rr => rr.IsBlocked);

    }
}
