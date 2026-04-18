using CampusCuisine.Models;

namespace CampusCuisine.Services;

/// <summary>
/// Decorator over <see cref="IApiService"/> that transparently caches the
/// last successful <see cref="IApiService.GetMenuByCategoryAsync"/> response
/// per category and silently falls back to it if a later call fails (network
/// down, backend unreachable, timeout). This satisfies the SHOULD
/// requirement for offline browsing of menu items without changing any
/// page, view-model, or the IApiService contract itself.
///
/// Only category listing is cached: <see cref="GetMenuItemAsync"/> and
/// <see cref="PostOrderAsync"/> pass through unchanged. Order placement
/// is deliberately not cacheable - the server is the price authority and
/// the only valid write path - and single-item lookup is used only by the
/// Today's pick card which already degrades silently on failure.
/// </summary>
public class CachedApiService : IApiService
{
  private readonly IApiService _inner;
  private readonly IMenuCache _cache;

  public CachedApiService(IApiService inner, IMenuCache cache)
  {
    _inner = inner;
    _cache = cache;
  }

  public async Task<List<MenuItemModel>> GetMenuByCategoryAsync(string category)
  {
    try
    {
      var items = await _inner.GetMenuByCategoryAsync(category);
      _cache.Save(category, items);
      return items;
    }
    catch
    {
      // Silent offline fallback: if we have a previously-cached list for
      // this category, return it and swallow the network error. If we
      // never cached anything (first launch with no connectivity), let
      // the original exception surface so category pages can show their
      // existing error state.
      var cached = _cache.Get(category);
      if (cached is not null)
        return cached;

      throw;
    }
  }

  public Task<MenuItemModel?> GetMenuItemAsync(int id) => _inner.GetMenuItemAsync(id);

  public Task<OrderConfirmationDto?> PostOrderAsync(CreateOrderRequestDto order) => _inner.PostOrderAsync(order);
}
