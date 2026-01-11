using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.IntegrationTests.Helpers;

public static class RestaurantTestData
{
    public static CreateRestaurantDto CreateValidRestaurantRequest(string? name = null)
    {
        var uniqueName = name ?? $"Закусочная \"Тесткейк\" {Guid.NewGuid().ToString()[..8]}";

        return new CreateRestaurantDto(
            Name: uniqueName,
            Description: "Тестирование...",
            PhoneNumber: "1234567890",
            Email: $"test{Guid.NewGuid().ToString()[..8]}@example.com",
            Address: new AddressDto("ул. Тестовая", "д. 1"),
            Location: new LocationDto(55.7558, 37.6173),
            OpenHours: new OpenHoursDto(
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(22, 0)
            ),
            AverageCheck: new MoneyDto(1500, "RUB"),
            CuisineTypes: new List<string>
            {
                CuisineType.Italian.Name,
                CuisineType.Georgian.Name
            }.AsReadOnly(),
            Tags: new List<string>
            {
                "Банкет",
                "Живая музыка",
                "Уютное место"
            }.AsReadOnly(),
            Images: null
        );
    }

    public static UpdateRestaurantDto CreateValidUpdateRequest(
        Guid restaurantId,
        string? name = null)
    {
        return new UpdateRestaurantDto(
            RestaurantId: restaurantId,
            Name: name ?? "Ресторан \"Обновлень\"",
            Description: "Обновление...",
            PhoneNumber: "9876543210",
            Email: "updated@example.com",
            Address: new AddressDto("ул. Обновленная", "д. 2"),
            Location: new LocationDto(55.7558, 37.6173),
            OpenHours: new OpenHoursDto(
                DayOfWeek.Monday,
                new TimeOnly(10, 0),
                new TimeOnly(23, 0)
            ),
            AverageCheck: new MoneyDto(2000, "RUB"),
            CuisineTypes: new List<string>
            {
                CuisineType.Russian.Name
            }.AsReadOnly(),
            Tags: new List<string>
            {
                "Банкет"
            }.AsReadOnly()
        );
    }
}
