using System.Collections.ObjectModel;
using CampusCuisine.Models;

namespace CampusCuisine.Pages;

public partial class DessertsPage : ContentPage
{
  public ObservableCollection<MenuItemModel> Items { get; set; }

  public DessertsPage()
  {
    InitializeComponent();

    Items = new ObservableCollection<MenuItemModel>()
    {
      new MenuItemModel
            {
                Name = "Chocolate Cake",
                Description = "Rich chocolate sponge with icing.",
                Price = "£3.25"
            },
            new MenuItemModel
            {
                Name = "Vanilla Ice Cream",
                Description = "Classic vanilla ice cream scoop.",
                Price = "£2.50"
            }
    };

    BindingContext = this;
  }
}