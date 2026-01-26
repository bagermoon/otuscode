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

        builder.Property(rr => rr.IsBlocked)
            .IsRequired();
    }
}
