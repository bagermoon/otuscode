using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.RestaurantService.Infrastructure.Data;

using TagEntity = RestoRate.RestaurantService.Domain.TagAggregate.Tag;

namespace RestoRate.Migrations.RestaurantSeeders;

public class TagSeeder : IDbSeeder<RestaurantDbContext>
{
    public async Task SeedAsync(RestaurantDbContext context)
    {
        if (await context.Set<TagEntity>().AnyAsync())
            return;

        var tags = new List<TagEntity>
        {
            new TagEntity("Уютное место"),
            new TagEntity("Живая музыка"),
            new TagEntity("Семейный отдых"),
            new TagEntity("Бизнес-ланч"),
            new TagEntity("Авторская кухня"),
            new TagEntity("Детская комната")
        };

        await context.Set<TagEntity>().AddRangeAsync(tags);
        await context.SaveChangesAsync();
    }
}
