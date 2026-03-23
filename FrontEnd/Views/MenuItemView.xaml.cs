using System.Collections.ObjectModel;
using CampusCuisine.Models;

namespace CampusCuisine.Views;

public partial class MenuItemView : ContentView
{
  public static readonly BindableProperty ItemsProperty =
      BindableProperty.Create(
          nameof(Items),
          typeof(ObservableCollection<MenuItemModel>),
          typeof(MenuItemView),
          new ObservableCollection<MenuItemModel>());

  public ObservableCollection<MenuItemModel> Items
  {
    get => (ObservableCollection<MenuItemModel>)GetValue(ItemsProperty);
    set => SetValue(ItemsProperty, value);
  }

  public MenuItemView()
  {
    InitializeComponent();
  }
}