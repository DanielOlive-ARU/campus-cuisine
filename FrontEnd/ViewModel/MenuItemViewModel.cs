using CampusCuisine.Models;
using System.Collections.ObjectModel;


namespace CampusCuisine.ViewModel
{
  public class MenuItemViewModel
  {
    public ObservableCollection<MenuItemModel> MenuItems { get; set; }

    public MenuItemViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemModel>
        {
        new MenuItemModel
        {
          Name = "Garlic bread",
          Description = "Toasted bread with garlic butter.",
          Price = "£3.50"
        },
        new MenuItemModel
        {
          Name = "Tomato Soup",
          Description = "Fresh tomato soup served hot.",
          Price = "£2.95"
        }
      };
    }
  }
}
