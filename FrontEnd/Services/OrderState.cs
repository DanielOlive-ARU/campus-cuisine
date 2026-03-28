using System.Collections.ObjectModel;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public class OrderState
{
  public ObservableCollection<MenuItemModel> Items { get; } = new();

  public void AddItem(MenuItemModel item)
  {
    Items.Add(item);
  }
}
