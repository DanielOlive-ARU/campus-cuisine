using System.Collections.ObjectModel;
using CampusCuisine.Models;

namespace CampusCuisine.Pages;

public partial class StartersPage : ContentPage
{
  public ObservableCollection<MenuItemModel> Items { get; set; }

  public StartersPage()
  {
    InitializeComponent();

    Items = new ObservableCollection<MenuItemModel>()
    {
      new MenuItemModel
            {
                Name = "Garlic Bread",
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

    BindingContext = this;
  }
}