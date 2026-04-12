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
    try
    {
      var response = await _httpClient.PostAsJsonAsync("api/orders", order);

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<OrderConfirmationDto>();
      }

      var errorBody = await response.Content.ReadAsStringAsync();
      var statusCode = (int)response.StatusCode;

      var message = statusCode switch
      {
        400 => "There was a problem with your order.",
        404 => "The requested item or order could not be found.",
        422 => "Some order details are invalid.",
        _ => $"Request failed with status {statusCode}."
      };

      if (!string.IsNullOrWhiteSpace(errorBody))
      {
        message += $"\n\nServer response: {errorBody}";
      }

      throw new ApiException(statusCode, message);
    }
    catch (HttpRequestException ex)
    {
      throw new ApiException(0, $"Network error: {ex.Message}");
    }
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
