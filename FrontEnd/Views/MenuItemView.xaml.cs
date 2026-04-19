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

  private void OnAddClicked(object? sender, EventArgs e)
  {
    if (sender is Button button &&
        button.CommandParameter is int menuItemId)
    {
      var card = DisplayItems.FirstOrDefault(c => c.Id == menuItemId);
      var name = card?.Name ?? string.Empty;
      var unitPrice = (double)(card?.Price ?? 0m);
      var description = card?.Description ?? string.Empty;

      _orderState.AddLine(menuItemId, name, unitPrice, description: description);
    }
  }

  private void OnDecreaseClicked(object? sender, EventArgs e)
  {
    if (sender is Button button &&
        button.CommandParameter is int menuItemId)
    {
      _orderState.RemoveLine(menuItemId);
    }
  }
}
