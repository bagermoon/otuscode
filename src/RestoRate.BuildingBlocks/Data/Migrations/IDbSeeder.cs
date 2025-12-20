using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Data.Migrations;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context, CancellationToken cancellationToken = default);
}
