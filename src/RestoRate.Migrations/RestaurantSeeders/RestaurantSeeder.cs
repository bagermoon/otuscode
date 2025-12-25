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
            url: "https://s.restorating.ru/places/3840x2160/places/41271/62fb79130c228.jpg",
            altText: "Интерьер основного зала La Pasta",
            displayOrder: 1,
            isPrimary: true
        );
        firstRestaurant.AddImage(
            url: "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/1a/39/68/9f/caption.jpg?w=600&h=600&s=1",
            altText: "Фирменная паста Карбонара",
            displayOrder: 2,
            isPrimary: false
        );
        firstRestaurant.AddImage(
            url: "https://www.tuttalavita.ru/images/about/about_01.webp",
            altText: "Дровяная печь",
            displayOrder: 3,
            isPrimary: false
        );

        firstRestaurant.SendToModeration();
        firstRestaurant.Publish();
        restaurants.Add(firstRestaurant);

        var secondRestaurant = new Restaurant(
            "Пхали-манали",
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
        TryAddTag(secondRestaurant, dbTags, "Детская комната");
        TryAddTag(secondRestaurant, dbTags, "Бизнес-ланч");

        secondRestaurant.AddImage(
            url: "https://scdn.tomesto.ru/img/place/000/029/503/restoran-georgia-armenia-gruziya-armeniya-na-ulitse-pushkina_43b3e_full-416935.jpg",
            altText: "Фасад ресторана Тбилиси",
            displayOrder: 1,
            isPrimary: true
        );
        secondRestaurant.AddImage(
            url: "https://img.freepik.com/premium-photo/blank-card-restaurant-table_1203353-41107.jpg?semt=ais_hybrid&w=740&q=80",
            altText: "Хинкали",
            displayOrder: 2,
            isPrimary: false
        );

        secondRestaurant.SendToModeration();
        secondRestaurant.Publish();
        restaurants.Add(secondRestaurant);

        var thirdRestaurant = new Restaurant(
            "Пельменная №1",
            "Русская кухня",
            new PhoneNumber("+7", "9991112233"),
            new Email("pel@sbp.rest"),
            new Address("г. Санкт-Петербург, пр. Невский, 15", "45"),
            new Location(55.776123, 37.632111),
            new OpenHours(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new Money(1800, "RUB")
        );
        thirdRestaurant.AddCuisineType(CuisineType.Russian);

        TryAddTag(thirdRestaurant, dbTags, "Бизнес-ланч");
        TryAddTag(firstRestaurant, dbTags, "Авторская кухня");
        TryAddTag(firstRestaurant, dbTags, "Роскошный интерьер");
        TryAddTag(firstRestaurant, dbTags, "Бесплатная парковка");

        thirdRestaurant.AddImage(
            url: "https://static.78.ru/images/uploads/1702637152759.jpg",
            altText: "#1",
            displayOrder: 1,
            isPrimary: true
        );
        thirdRestaurant.AddImage(
            url: "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/06/77/45/43/caption.jpg?w=1800&h=1000&s=1",
            altText: "#2",
            displayOrder: 2,
            isPrimary: false
        );
        thirdRestaurant.AddImage(
            url: "https://account.spb.ru/upload/22358/photo.jpg",
            altText: "#3",
            displayOrder: 3,
            isPrimary: false
        );

        thirdRestaurant.SendToModeration();
        thirdRestaurant.Publish();
        restaurants.Add(thirdRestaurant);

        var fourthRestaurant = new Restaurant(
            "Суши-бар Аригато-ми",
            "Японская кухня",
            new PhoneNumber("+7", "9991112233"),
            new Email("sushiArigato@kz.rest"),
            new Address("г. Казань, ул. Ленина, 19", "17"),
            new Location(55.776123, 37.632111),
            new OpenHours(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new Money(1800, "RUB")
        );
        fourthRestaurant.AddCuisineType(CuisineType.Japanese);

        TryAddTag(fourthRestaurant, dbTags, "Суши-бар");
        TryAddTag(fourthRestaurant, dbTags, "Бизнес-ланч");
        TryAddTag(fourthRestaurant, dbTags, "Бесплатная парковка");

        fourthRestaurant.AddImage(
            url: "https://www.allpbspb.ru/upload/resize_cache/iblock/d95/700_700_0/d951bec0443691a374489c8c7bcd1a1b.jpg",
            altText: "#1",
            displayOrder: 1,
            isPrimary: true
        );
        fourthRestaurant.AddImage(
            url: "https://i.pinimg.com/736x/d3/c6/df/d3c6dff867a80a7e91cc5391312bd398.jpg",
            altText: "#2",
            displayOrder: 2,
            isPrimary: false
        );

        fourthRestaurant.SendToModeration();
        fourthRestaurant.Publish();
        restaurants.Add(fourthRestaurant);

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
