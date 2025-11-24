using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.DTOs.CRUD;
using RestoRate.Restaurant.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.UnitTests.Helpers;

public static class TestDataBuilder
{
    #region RestaurantDto
    public static CreateRestaurantDto CreateValidRestaurantDto(
        string? name = null,
        string? description = null,
        string? phoneNumber = null,
        string? email = null,
        IEnumerable<string>? cuisineTypes = null,
        IEnumerable<string>? tags = null,
        IEnumerable<CreateRestaurantImageDto>? images = null)
    {
        return new CreateRestaurantDto(
            Name: name ?? "Закусочная \"Тесткейк\"",
            Description: description ?? "Тестирование...",
            PhoneNumber: phoneNumber ?? "1234567890",
            Email: email ?? "test@example.com",
            Address: new AddressDto("ул. Пушкина", "д. 10"),
            Location: new LocationDto(55.7558, 37.6173),
            OpenHours: new OpenHoursDto(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(22, 0)
            ),

            AverageCheck: new MoneyDto(1500, "RUB"),

            CuisineTypes: (cuisineTypes?.ToList() ?? new List<string>
            {
                CuisineType.Italian.Name,
                CuisineType.Georgian.Name
            }).AsReadOnly(),

            Tags: (tags?.ToList() ?? new List<string>
            {
                Tag.Banquet.Name,
                Tag.LiveMusic.Name,
                Tag.SummerTerrace.Name
            }).AsReadOnly(),

            Images: images?.ToList().AsReadOnly()
        );
    }

    public static UpdateRestaurantDto CreateValidUpdateRestaurantDto(
        Guid? restaurantId = null,
        string? name = null,
        IEnumerable<string>? cuisineTypes = null,
        IEnumerable<string>? tags = null)
    {
        return new UpdateRestaurantDto(
            RestaurantId: restaurantId ?? Guid.NewGuid(),
            Name: name ?? "Ресторан \"Обновлень\"",
            Description: "Обновление...",
            PhoneNumber: "9876543210",
            Email: "updated@example.com",
            Address: new AddressDto("ул. Ленина", "д. 25"),
            Location: new LocationDto(55.7558, 37.6173),
            OpenHours: new OpenHoursDto(
                DayOfWeek.Monday,
                new TimeOnly(10, 0),
                new TimeOnly(23, 0)
            ),
            AverageCheck: new MoneyDto(2000, "RUB"),
            CuisineTypes: (cuisineTypes?.ToList() ?? new List<string>
            {
                CuisineType.Georgian.Name,
                CuisineType.Russian.Name
            }).AsReadOnly(),
            Tags: (tags?.ToList() ?? new List<string>
            {
                Tag.Wedding.Name,
                Tag.CorporateParty.Name
            }).AsReadOnly()
        );
    }

    public static List<string> GetRandomCuisineTypes(int count = 2)
    {
        var allCuisines = CuisineType.List.Select(c => c.Name).ToList();
        return allCuisines.Take(count).ToList();
    }

    public static List<string> GetRandomTags(int count = 3)
    {
        var allTags = Tag.List.Select(t => t.Name).ToList();
        return allTags.Take(count).ToList();
    }

    public static CreateRestaurantImageDto CreateValidImageDto(
        string? url = null,
        string? altText = null,
        bool isPrimary = false)
    {
        return new CreateRestaurantImageDto(
            Url: url ?? "https://example.com/restaurant.jpg",
            AltText: altText ?? "Restaurant image",
            IsPrimary: isPrimary
        );
    }
    #endregion
    #region RestaurantEntity

    /// <summary> Создает агрегат для unit тестов </summary>
    public static RestaurantEntity CreateRestaurantEntity(Guid id, string name)
    {
        var phoneNumber = new PhoneNumber("+7", "1234567890");
        var email = new Email("test@example.com");
        var address = new Address("ул. Тестовая", "д. 1");
        var location = new Location(55.7558, 37.6173);
        var openHours = new OpenHours(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(22, 0));
        var averageCheck = new Money(1500, "RUB");

        var restaurant = new RestaurantEntity(
            name,
            "Test description",
            phoneNumber,
            email,
            address,
            location,
            openHours,
            averageCheck
        );

        SetPrivatePropertyValue(restaurant, "Id", id);

        restaurant.AddCuisineType(CuisineType.Italian);
        restaurant.AddTag(Tag.Banquet);
        restaurant.ClearDomainEvents();

        return restaurant;
    }

    public static RestaurantEntity CreateRestaurantEntityWithImages(Guid id)
    {
        var restaurant = CreateRestaurantEntity(id, "Restaurant with images");

        restaurant.AddImage(
            url: "https://example.com/image1.jpg",
            altText: "Primary Image",
            displayOrder: 1,
            isPrimary: true
        );

        restaurant.AddImage(
            url: "https://example.com/image2.jpg",
            altText: "Secondary Image",
            displayOrder: 2,
            isPrimary: false
        );

        restaurant.AddImage(
            url: "https://example.com/image3.jpg",
            altText: "Third Image",
            displayOrder: 3,
            isPrimary: false
        );

        restaurant.ClearDomainEvents();
        return restaurant;
    }

    public static RestaurantEntity CreateRestaurantEntityWithCuisines(
        Guid id,
        string name,
        params CuisineType[] cuisineTypes)
    {
        var restaurant = CreateRestaurantEntity(id, name);

        restaurant.UpdateCuisineTypes(cuisineTypes);
        restaurant.ClearDomainEvents();
        return restaurant;
    }

    public static RestaurantEntity CreateRestaurantEntityWithTags(
        Guid id,
        string name,
        params Tag[] tags)
    {
        var restaurant = CreateRestaurantEntity(id, name);

        restaurant.UpdateTags(tags);
        restaurant.ClearDomainEvents();
        return restaurant;
    }

    public static List<RestaurantEntity> CreateRestaurantList(int count)
    {
        var restaurants = new List<RestaurantEntity>();

        for (int i = 1; i <= count; i++)
        {
            restaurants.Add(CreateRestaurantEntity(
                Guid.NewGuid(),
                $"Restaurant {i}"
            ));
        }

        return restaurants;
    }

    /// <summary> Вспомогательный метод для установки приватных свойств через рефлексию </summary>
    private static void SetPrivatePropertyValue(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null)
        {
            property.SetValue(obj, value);
        }
    }

    #endregion
}
