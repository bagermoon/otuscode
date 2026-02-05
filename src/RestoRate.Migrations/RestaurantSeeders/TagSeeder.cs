using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.RestaurantService.Infrastructure.Data;

using TagEntity = RestoRate.RestaurantService.Domain.TagAggregate.Tag;

namespace RestoRate.Migrations.RestaurantSeeders;

public class TagSeeder : IDbSeeder<RestaurantDbContext>
{
    public async Task SeedAsync(RestaurantDbContext context, CancellationToken ct = default)
    {
        if (await context.Set<TagEntity>().AnyAsync(ct))
            return;

        var tags = new List<TagEntity>
        {
            new TagEntity("Уютное место"),
            new TagEntity("Живая музыка"),
            new TagEntity("Семейный отдых"),
            new TagEntity("Бизнес-ланч"),
            new TagEntity("Авторская кухня"),
            new TagEntity("Детская комната"),
            new TagEntity("Кофейня"),
            new TagEntity("Крафтовое пиво"),
            new TagEntity("Веганское меню"),
            new TagEntity("Стейк-хаус"),
            new TagEntity("Суши-бар"),
            new TagEntity("Панорамный вид"),
            new TagEntity("Летняя терраса"),
            new TagEntity("Каминный зал"),
            new TagEntity("Роскошный интерьер"),
            new TagEntity("Спортивные трансляции"),
            new TagEntity("Проведение банкетов"),
            new TagEntity("Мастер-классы шефа"),
            new TagEntity("Винная карта"),
            new TagEntity("Бесплатная парковка"),
            new TagEntity("Стрит-фуд"),
            new TagEntity("Мясной ресторан")
        };

        await context.Set<TagEntity>().AddRangeAsync(tags, ct);
        await context.SaveChangesAsync(ct);
    }
}
