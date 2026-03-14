using System.Collections.ObjectModel;
using CampusCuisine.Models;

namespace CampusCuisine.Pages;

public partial class MainsPage : ContentPage
{
  public ObservableCollection<MenuItemModel> Items { get; set; }

  public MainsPage()
  {
    InitializeComponent();

    Items = new ObservableCollection<MenuItemModel>()
    {
      new MenuItemModel
            {
                Name = "Chicken Burger",
                Description = "Grilled chicken burger with fries.",
                Price = "£6.99"
            },
            new MenuItemModel
            {
                Name = "Spaghetti Bolognese",
                Description = "Pasta with rich beef tomato sauce.",
                Price = "£7.50"
            }
    };

    BindingContext = this;
  }
}