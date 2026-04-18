using CampusCuisine.Models;

namespace CampusCuisine.Services;

public interface IApiService
{
  Task<List<MenuItemModel>> GetMenuByCategoryAsync(string category);
  Task<MenuItemModel?> GetMenuItemAsync(int id);
  Task<OrderConfirmationDto?> PostOrderAsync(CreateOrderRequestDto order);
}