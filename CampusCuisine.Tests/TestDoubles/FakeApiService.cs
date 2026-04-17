using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.Tests.TestDoubles;

public class FakeApiService : IApiService
{
  public Func<string, Task<List<MenuItemModel>>>? GetMenuByCategoryAsyncHandler { get; set; }
  public Func<int, Task<MenuItemModel?>>? GetMenuItemAsyncHandler { get; set; }
  public Func<CreateOrderRequestDto, Task<OrderConfirmationDto?>>? PostOrderAsyncHandler { get; set; }

  public int GetMenuByCategoryAsyncCallCount { get; private set; }
  public string? LastCategory { get; private set; }

  public Task<List<MenuItemModel>> GetMenuByCategoryAsync(string category)
  {
    GetMenuByCategoryAsyncCallCount++;
    LastCategory = category;

    if (GetMenuByCategoryAsyncHandler is not null)
      return GetMenuByCategoryAsyncHandler(category);

    return Task.FromResult(new List<MenuItemModel>());
  }

  public Task<MenuItemModel?> GetMenuItemAsync(int id)
  {
    if (GetMenuItemAsyncHandler is not null)
      return GetMenuItemAsyncHandler(id);

    return Task.FromResult<MenuItemModel?>(null);
  }

  public Task<OrderConfirmationDto?> PostOrderAsync(CreateOrderRequestDto order)
  {
    if (PostOrderAsyncHandler is not null)
      return PostOrderAsyncHandler(order);

    return Task.FromResult<OrderConfirmationDto?>(null);
  }
}
