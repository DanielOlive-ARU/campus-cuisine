using System.Text.Json;
using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Maui.Storage;

namespace CampusCuisine;

/// <summary>
/// MAUI-backed <see cref="IMenuCache"/> implementation that persists the last
/// successful menu response per category using <see cref="Preferences"/>.
/// Preferences is a small cross-platform key/value store that survives app
/// restart and is sized appropriately for the seeded menu (~12 items).
/// Serialisation uses System.Text.Json so the cached payload matches the
/// shape returned by the backend directly.
///
/// If the stored JSON cannot be deserialised (for example because the
/// MenuItemModel contract has been extended since the cache was written),
/// <see cref="Get"/> returns null so the decorator behaves as though no
/// cache existed and the caller falls back to its normal error path.
/// </summary>
public class PreferencesMenuCache : IMenuCache
{
  private const string KeyPrefix = "menu.cached.";

  public List<MenuItemModel>? Get(string category)
  {
    var json = Preferences.Default.Get(KeyPrefix + category, string.Empty);
    if (string.IsNullOrEmpty(json))
      return null;

    try
    {
      return JsonSerializer.Deserialize<List<MenuItemModel>>(json);
    }
    catch (JsonException)
    {
      return null;
    }
  }

  public void Save(string category, List<MenuItemModel> items)
  {
    var json = JsonSerializer.Serialize(items);
    Preferences.Default.Set(KeyPrefix + category, json);
  }
}
