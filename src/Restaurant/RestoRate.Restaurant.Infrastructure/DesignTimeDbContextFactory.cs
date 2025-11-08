using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using RestoRate.Common;

namespace RestoRate.Restaurant.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
{
    public RestaurantDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = config.GetConnectionString(AppHostProjects.RestaurantDb);
            
        var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString:connectionString)
            .UseSnakeCaseNamingConvention()
            ;

        return new RestaurantDbContext(optionsBuilder.Options);
    }
}
