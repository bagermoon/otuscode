using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.DbContexts;

using ReviewEntity = RestoRate.Review.Domain.ReviewAggregate.Review;

namespace RestoRate.Review.Infrastructure.Data;

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
