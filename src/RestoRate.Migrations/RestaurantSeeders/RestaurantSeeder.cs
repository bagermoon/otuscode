using Microsoft.EntityFrameworkCore;

using NodaMoney;

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

        var r1 = new Restaurant(
            "La Pasta",
            "Итальянский ресторан с домашней пастой.",
            new PhoneNumber("+7", "9990001122"),
            new Email("info@lapasta.ru"),
            new Address("г. Москва, ул. Ленина, 10", "10"),
            new Location(55.751244, 37.618423),
            CreateDailySchedule(new TimeOnly(10, 0), new TimeOnly(23, 00)),
            new Money(2500m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r1.AddCuisineType(CuisineType.Italian);
        TryAddTag(r1, dbTags, "Уютное место");
        TryAddTag(r1, dbTags, "Семейный отдых");

        r1.AddImage("https://s.restorating.ru/places/3840x2160/places/41271/62fb79130c228.jpg", "Интерьер", 1, true);
        r1.AddImage("https://dynamic-media-cdn.tripadvisor.com/media/photo-o/1a/39/68/9f/caption.jpg?w=600&h=600&s=1", "Карбонара", 2);
        r1.AddImage("https://www.tuttalavita.ru/images/about/about_01.webp", "Печь", 3);

        AddRatings(r1, 4.5m, 120);
        restaurants.Add(r1);

        var r2 = new Restaurant(
            "Пхали-манали",
            "Аутентичная грузинская кухня и вино.",
            new PhoneNumber("+7", "9991112233"),
            new Email("geo@tbilisi.rest"),
            new Address("г. Москва, пр. Мира, 5", "5"),
            new Location(55.776123, 37.632111),
            CreateDailySchedule(new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new Money(1800m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r2.AddCuisineType(CuisineType.Georgian);
        TryAddTag(r2, dbTags, "Живая музыка");
        TryAddTag(r2, dbTags, "Детская комната");

        r2.AddImage("https://scdn.tomesto.ru/img/place/000/029/503/restoran-georgia-armenia-gruziya-armeniya-na-ulitse-pushkina_43b3e_full-416935.jpg", "Фасад", 1, true);
        r2.AddImage("https://img.freepik.com/premium-photo/blank-card-restaurant-table_1203353-41107.jpg", "Хинкали", 2);

        AddRatings(r2, 4.7m, 85);
        restaurants.Add(r2);

        var r3 = new Restaurant(
            "Пельменная №1",
            "Традиционная русская кухня в современном исполнении.",
            new PhoneNumber("+7", "9993334455"),
            new Email("pel@sbp.rest"),
            new Address("г. Санкт-Петербург, пр. Невский, 15", "45"),
            new Location(59.9311, 30.3609),
            CreateDailySchedule(new TimeOnly(09, 0), new TimeOnly(21, 00)),
            new Money(1200m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r3.AddCuisineType(CuisineType.Russian);
        TryAddTag(r3, dbTags, "Бизнес-ланч");
        TryAddTag(r3, dbTags, "Авторская кухня");

        r3.AddImage("https://static.78.ru/images/uploads/1702637152759.jpg", "Зал", 1, true);
        r3.AddImage("https://account.spb.ru/upload/22358/photo.jpg", "Блюдо", 2);

        AddRatings(r3, 4.2m, 200);
        restaurants.Add(r3);

        var r4 = new Restaurant(
            "Суши-бар Аригато-ми",
            "Свежайшие суши и роллы из под ножа.",
            new PhoneNumber("+7", "9995556677"),
            new Email("sushi@arigato.rest"),
            new Address("г. Казань, ул. Баумана, 19", "17"),
            new Location(55.7887, 49.1221),
            CreateDailySchedule(new TimeOnly(11, 0), new TimeOnly(23, 00)),
            new Money(1500m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r4.AddCuisineType(CuisineType.Japanese);
        TryAddTag(r4, dbTags, "Суши-бар");
        TryAddTag(r4, dbTags, "Бесплатная парковка");

        r4.AddImage("https://www.allpbspb.ru/upload/resize_cache/iblock/d95/700_700_0/d951bec0443691a374489c8c7bcd1a1b.jpg", "Интерьер", 1, true);
        r4.AddImage("https://i.pinimg.com/736x/d3/c6/df/d3c6dff867a80a7e91cc5391312bd398.jpg", "Сет роллов", 2);

        AddRatings(r4, 4.0m, 15);
        restaurants.Add(r4);

        var r5 = new Restaurant(
            "Burger Heroes",
            "Крафтовые бургеры и стейки на открытом огне.",
            new PhoneNumber("+7", "9998887766"),
            new Email("hero@burger.com"),
            new Address("г. Екатеринбург, ул. Вайнера, 12", "1"),
            new Location(56.8389, 60.6057),
            CreateDailySchedule(new TimeOnly(12, 0), new TimeOnly(23, 00)),
            new Money(1300m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r5.AddCuisineType(CuisineType.American);
        TryAddTag(r5, dbTags, "Стрит-фуд");
        TryAddTag(r5, dbTags, "Мясной ресторан");

        r5.AddImage("https://avatars.mds.yandex.net/get-altay/11018317/2a0000018c0ac3c101bc497a6a7836561499/XXL_height", "Барная стойка", 1, true);
        r5.AddImage("https://avatars.mds.yandex.net/get-altay/10830675/2a0000018c8801293ea39c80c5138219714a/orig", "Бургер", 2);

        AddRatings(r5, 4.8m, 350);
        restaurants.Add(r5);

        var r6 = new Restaurant(
            "Le Petit Paris",
            "Маленькая Франция в центре города. Вино и круассаны.",
            new PhoneNumber("+7", "9992223344"),
            new Email("bonjour@paris.ru"),
            new Address("г. Нижний Новгород, ул. Рождественская, 24", "2"),
            new Location(56.3269, 44.0059),
            CreateDailySchedule(new TimeOnly(08, 0), new TimeOnly(22, 00)),
            new Money(3000m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r6.AddCuisineType(CuisineType.French);
        TryAddTag(r6, dbTags, "Винная карта");
        TryAddTag(r6, dbTags, "Живая музыка");

        r6.AddImage("https://avatars.mds.yandex.net/i?id=672e3b80d0da65a24d1666c8ca2e8225_l-5433424-images-thumbs&n=13", "Зал", 1, true);
        r6.AddImage("https://img.freepik.com/premium-photo/design-french-style-vintage-partment-dining-room_916191-202957.jpg?semt=ais_hybrid&w=740", "Завтрак", 2);
        r6.AddImage("https://media-cdn.tripadvisor.com/media/photo-m/1280/17/45/4e/fb/le-clarence.jpg", "Разное", 3);

        AddRatings(r6, 4.9m, 42);
        restaurants.Add(r6);

        var r7 = new Restaurant(
            "Mama Roma",
            "Домашняя пицца из дровяной печи.",
            new PhoneNumber("+7", "9990009988"),
            new Email("pizza@mama.rom"),
            new Address("г. Сочи, Курортный проспект, 50", "1"),
            new Location(43.5808, 39.7203),
            CreateDailySchedule(new TimeOnly(12, 0), new TimeOnly(23, 30)),
            new Money(1600m, Currency.FromCode("RUB")),
            Guid.NewGuid()
        );
        r7.AddCuisineType(CuisineType.Italian);
        TryAddTag(r7, dbTags, "Детская комната");
        TryAddTag(r7, dbTags, "Семейный отдых");

        r7.AddImage("https://www.gdebar.ru/data/app/bar/img/gallery/6829/194504.webp", "Интерьер", 1, true);
        r7.AddImage("https://i.pinimg.com/736x/39/fd/13/39fd132c42d02b72db85192d15f48544.jpg", "Пицца", 2);

        AddRatings(r7, 4.1m, 60);
        restaurants.Add(r7);

        await context.Restaurants.AddRangeAsync(restaurants, ct);
        await context.SaveChangesAsync(ct);
    }

    private static List<OpenHours> CreateDailySchedule(TimeOnly open, TimeOnly close)
    {
        var schedule = new List<OpenHours>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            schedule.Add(new OpenHours(day, open, close, isClosed: false));
        }
        return schedule;
    }

    private static void TryAddTag(Restaurant restaurant, List<TagEntity> availableTags, string tagName)
    {
        var tag = availableTags.FirstOrDefault(t => t.Name == tagName);
        if (tag != null)
            restaurant.AddTag(tag);
    }

    private Restaurant AddRatings(
        Restaurant restaurant,
        decimal approvedAverageRating = 4.5m,
        int approvedReviewsCount = 120
    )
    {
        restaurant.UpdateRatings(
            approvedAverageRating: approvedAverageRating,
            approvedReviewsCount: approvedReviewsCount,
            approvedAverageCheck: restaurant.AverageCheck,
            provisionalAverageRating: 4.3m,
            provisionalReviewsCount: 30,
            provisionalAverageCheck: Money.Zero
        );

        return restaurant;
    }
}
