using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

using TagEntity = RestoRate.RestaurantService.Domain.TagAggregate.Tag;

namespace RestoRate.Migrations.RestaurantSeeders;

public class RestaurantSeeder() : IDbSeeder<RestaurantDbContext>
{
    public async Task SeedAsync(RestaurantDbContext context, CancellationToken ct = default)
    {
        if (await context.Restaurants.AnyAsync(ct))
            return;

        List<TagEntity> dbTags = await context.Set<TagEntity>().ToListAsync(ct);

        var restaurants = new List<Restaurant>();

        var firstRestaurant = new Restaurant(
            "La Pasta",
            "Итальянский ресторан.",
            new PhoneNumber("+7", "9990001122"),
            new Email("info@lapasta.ru"),
            new Address("г. Москва, ул. Ленина, 10", "10"),
            new Location(55.751244, 37.618423),
            new OpenHours(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(23, 59)),
            new Money(2500, "RUB")
        );
        firstRestaurant.AddCuisineType(CuisineType.Italian);

        TryAddTag(firstRestaurant, dbTags, "Уютное место");
        TryAddTag(firstRestaurant, dbTags, "Семейный отдых");

        firstRestaurant.AddImage(
            url: "https://raw.githubusercontent.com/restorate/assets/main/lapasta-main.jpg",
            altText: "Интерьер основного зала La Pasta",
            displayOrder: 1,
            isPrimary: true
        );
        firstRestaurant.AddImage(
            url: "https://raw.githubusercontent.com/restorate/assets/main/lapasta-food.jpg",
            altText: "Фирменная паста Карбонара",
            displayOrder: 2,
            isPrimary: false
        );
        firstRestaurant.AddImage(
            url: "https://raw.githubusercontent.com/restorate/assets/main/lapasta-oven.jpg",
            altText: "Дровяная печь",
            displayOrder: 3,
            isPrimary: false
        );

        firstRestaurant.SendToModeration();
        firstRestaurant.Publish();
        restaurants.Add(firstRestaurant);

        var secondRestaurant = new Restaurant(
            "Тбилиси",
            "Грузинская кухня.",
            new PhoneNumber("+7", "9991112233"),
            new Email("geo@tbilisi.rest"),
            new Address("г. Москва, пр. Мира, 5", "5"),
            new Location(55.776123, 37.632111),
            new OpenHours(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new Money(1800, "RUB")
        );
        secondRestaurant.AddCuisineType(CuisineType.Georgian);

        TryAddTag(secondRestaurant, dbTags, "Живая музыка");

        secondRestaurant.AddImage(
            url: "https://raw.githubusercontent.com/restorate/assets/main/tbilisi-ext.jpg",
            altText: "Фасад ресторана Тбилиси",
            displayOrder: 1,
            isPrimary: true
        );
        secondRestaurant.AddImage(
            url: "https://raw.githubusercontent.com/restorate/assets/main/tbilisi-khinkali.jpg",
            altText: "Хинкали",
            displayOrder: 2,
            isPrimary: false
        );

        secondRestaurant.SendToModeration();
        secondRestaurant.Publish();
        restaurants.Add(secondRestaurant);

        await context.Restaurants.AddRangeAsync(restaurants, ct);
        await context.SaveChangesAsync(ct);
    }

    private static void TryAddTag(
        Restaurant restaurant,
        List<TagEntity> availableTags,
        string tagName)
    {
        var tag = availableTags.FirstOrDefault(t => t.Name == tagName);
        if (tag != null)
            restaurant.AddTag(tag);
    }
}
