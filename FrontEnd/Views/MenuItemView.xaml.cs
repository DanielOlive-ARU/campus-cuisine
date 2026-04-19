using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Views;

public partial class MenuItemView : ContentView
{
  private readonly IOrderStateService _orderState;
  private MenuItemCardSync? _sync;

  public static readonly BindableProperty ItemsProperty =
      BindableProperty.Create(
          nameof(Items),
          typeof(ObservableCollection<MenuItemModel>),
          typeof(MenuItemView),
          defaultValueCreator: _ => new ObservableCollection<MenuItemModel>(),
          propertyChanged: OnItemsChanged);

  public ObservableCollection<MenuItemModel> Items
  {
    get => (ObservableCollection<MenuItemModel>)GetValue(ItemsProperty);
    set => SetValue(ItemsProperty, value);
  }

  public ObservableCollection<MenuItemCardViewModel> DisplayItems { get; } = new();

  public MenuItemView()
  {
    InitializeComponent();
    _orderState = App.Services.GetRequiredService<IOrderStateService>();

    RebuildSync();
  }

  private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
  {
    if (bindable is MenuItemView view)
    {
      view.RebuildSync();
    }
  }

  private void RebuildSync()
  {
    _sync?.Dispose();
    _sync = null;
    DisplayItems.Clear();

    if (Items is not null)
    {
      _sync = new MenuItemCardSync(Items, _orderState, DisplayItems);
    }
  }
}
