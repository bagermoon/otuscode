using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.DTOs.CRUD;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.Restaurant.UnitTests.Helpers;

public static class TestDataBuilder
{
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
            Name: name ?? "Test Restaurant",
            Description: description ?? "Test Description",
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
            Name: name ?? "Updated Restaurant",
            Description: "Updated Description",
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
            CuisineTypes: cuisineTypes?.ToList() ?? new List<string>
            {
                CuisineType.Georgian.Name,
                CuisineType.Russian.Name
            },
            Tags: tags?.ToList() ?? new List<string>
            {
                Tag.Wedding.Name,
                Tag.CorporateParty.Name
            }
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
}
