using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RestoRate.BuildingBlocks.Data.DbContexts;
namespace RestoRate.RestaurantService.Infrastructure.Data;

public class RestaurantDbContext : DbContextBase
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
    { }

    public DbSet<Domain.RestaurantAggregate.Restaurant> Restaurants { get; set; }
    public DbSet<Domain.RestaurantAggregate.RestaurantImage> Images { get; set; }
    public DbSet<Domain.RestaurantAggregate.RestaurantCuisineType> CuisineTypes { get; set; }
    public DbSet<Domain.RestaurantAggregate.RestaurantTag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    }
}
