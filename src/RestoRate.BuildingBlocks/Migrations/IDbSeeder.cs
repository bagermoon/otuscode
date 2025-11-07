using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Migrations;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
