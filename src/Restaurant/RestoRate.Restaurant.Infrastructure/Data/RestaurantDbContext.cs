using Microsoft.EntityFrameworkCore;

using RestoRate.Restaurant.Infrastructure.Configuration;

namespace RestoRate.Restaurant.Infrastructure.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.RestaurantAggregate.Restaurant> Restaurants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RestaurantConfiguration());

    }
}
