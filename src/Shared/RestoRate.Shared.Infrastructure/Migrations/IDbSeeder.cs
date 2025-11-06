using Microsoft.EntityFrameworkCore;

namespace RestoRate.Shared.Infrastructure.Migrations;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
