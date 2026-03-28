using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;

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

  private void OnAddClicked(object? sender, EventArgs e)
  {
    if (sender is Button button &&
        button.CommandParameter is MenuItemModel item)
    {
      var orderState = Application.Current!.Services.GetRequiredService<OrderState>();
      orderState.AddItem(item);
    }
  }
}