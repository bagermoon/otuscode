using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

namespace RestoRate.BlazorDashboard.Services;

public class RestaurantWebService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("RestaurantApi");

    public async Task<PagedResult<RestaurantDto>?> GetRestaurantsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchTerm = null,
        string? cuisineType = null,
        string? tag = null)
    {
        var query = $"/restaurants?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (!string.IsNullOrWhiteSpace(cuisineType))
            query += $"&cuisineType={Uri.EscapeDataString(cuisineType)}";
        if (!string.IsNullOrWhiteSpace(tag))
            query += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await _httpClient.GetAsync(query);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PagedResult<RestaurantDto>>();
    }
}
