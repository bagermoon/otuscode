using System.Reflection;

using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.DbContexts;

using ReviewEntity = RestoRate.ReviewService.Domain.ReviewAggregate.Review;

namespace RestoRate.ReviewService.Infrastructure.Data;

public class ReviewDbContext : DbContextBase
{
    public DbSet<ReviewEntity> Reviews { get; set; }
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
