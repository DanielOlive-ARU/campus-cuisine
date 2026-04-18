using CampusCuisine.Models;

namespace CampusCuisine.Services;

/// <summary>
/// Per-category menu cache used by <see cref="CachedApiService"/> to fall back
/// to a last-successful menu response when the backend is unreachable.
/// Implementations are platform-specific; the reference MAUI implementation
/// (<c>PreferencesMenuCache</c>) is backed by <c>Preferences</c>.
/// </summary>
public interface IMenuCache
{
  /// <summary>
  /// Return the last cached list of items for the given category, or null
  /// if nothing has been cached yet (or the cached value cannot be read).
  /// </summary>
  List<MenuItemModel>? Get(string category);

  /// <summary>
  /// Persist the given list as the last-known-good cache for the category.
  /// Replaces any previously cached value for that category.
  /// </summary>
  void Save(string category, List<MenuItemModel> items);
}
