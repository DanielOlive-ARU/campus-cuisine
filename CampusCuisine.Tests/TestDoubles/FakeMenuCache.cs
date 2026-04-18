using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.Tests.TestDoubles;

/// <summary>
/// In-memory <see cref="IMenuCache"/> double for unit tests. Exposes the
/// underlying store directly so tests can seed or inspect cached state.
/// </summary>
public class FakeMenuCache : IMenuCache
{
  public Dictionary<string, List<MenuItemModel>> Store { get; } = new();

  public List<MenuItemModel>? Get(string category)
  {
    return Store.TryGetValue(category, out var items) ? items : null;
  }

  public void Save(string category, List<MenuItemModel> items)
  {
    Store[category] = items;
  }
}
