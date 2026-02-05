using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;
using RestoRate.ServiceDefaults;

namespace RestoRate.BlazorDashboard.Services;

public class RestaurantWebService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(AppHostProjects.Gateway);

    public async Task<Contracts.Common.PagedResult<RestaurantDto>?> GetRestaurantsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? searchTerm = null,
        string? cuisineType = null,
        string? tag = null)
    {
        var query = $"restaurants?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (!string.IsNullOrWhiteSpace(cuisineType))
            query += $"&cuisineType={Uri.EscapeDataString(cuisineType)}";
        if (!string.IsNullOrWhiteSpace(tag))
            query += $"&tag={Uri.EscapeDataString(tag)}";

        var response = await _httpClient.GetAsync(query);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Contracts.Common.PagedResult<RestaurantDto>>();
    }

    public async Task<RestaurantDto?> GetRestaurantByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<RestaurantDto>($"restaurants/{id}");
    }

    public async Task<List<TagDto>> GetTagsAsync()
    {
        var response = await _httpClient.GetAsync("restaurants/tags");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TagDto>>() ?? new();
    }

    public async Task<bool> CreateRestaurantAsync(CreateRestaurantDto dto)
    {
        var client = httpClientFactory.CreateClient(AppHostProjects.Gateway);
        var response = await client.PostAsJsonAsync("restaurants", dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateRestaurantAsync(Guid id, CreateRestaurantDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"restaurants/{id}", dto);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRestaurantAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"restaurants/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<RestaurantDto>> GetUserRestaurantsByOwnerAsync(string ownerId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<RestaurantDto>>($"restaurants/owner/{ownerId}");
        return result ?? new List<RestaurantDto>();
    }
}
