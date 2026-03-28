using System.Net.Http.Json;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public class ApiService : IApiService
{
  private readonly HttpClient _httpClient;

  public ApiService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<List<MenuItemModel>> GetMenuByCategoryAsync(string category)
  {
    var encodedCategory = Uri.EscapeDataString(category);
    var items = await _httpClient.GetFromJsonAsync<List<MenuItemModel>>($"api/menu?category={encodedCategory}")
                ?? new List<MenuItemModel>();

    FixImageUrls(items);
    return items;
  }

  public async Task<MenuItemModel?> GetMenuItemAsync(int id)
  {
    var item = await _httpClient.GetFromJsonAsync<MenuItemModel>($"api/menu/{id}");

    if (item is not null)
    {
      FixImageUrl(item);
    }

    return item;
  }

  public async Task<OrderConfirmationDto?> PostOrderAsync(CreateOrderRequestDto order)
  {
    var response = await _httpClient.PostAsJsonAsync("api/orders", order);

    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    return await response.Content.ReadFromJsonAsync<OrderConfirmationDto>();
  }

  private void FixImageUrls(IEnumerable<MenuItemModel> items)
  {
    foreach (var item in items)
    {
      FixImageUrl(item);
    }
  }

  private void FixImageUrl(MenuItemModel item)
  {
    if (string.IsNullOrWhiteSpace(item.ImageUrl) || _httpClient.BaseAddress is null)
    {
      return;
    }

    if (Uri.TryCreate(item.ImageUrl, UriKind.Absolute, out _))
    {
      return;
    }

    item.ImageUrl = new Uri(_httpClient.BaseAddress, item.ImageUrl).ToString();
  }
}
