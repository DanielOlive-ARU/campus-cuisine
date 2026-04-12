using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace CampusCuisine.Views;

public partial class MenuItemView : ContentView, INotifyPropertyChanged
{
  private readonly OrderState _orderState;

  public static readonly BindableProperty ItemsProperty =
      BindableProperty.Create(
          nameof(Items),
          typeof(ObservableCollection<MenuItemModel>),
          typeof(MenuItemView),
          new ObservableCollection<MenuItemModel>(),
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
    _orderState = App.Services.GetRequiredService<OrderState>();
    // Do NOT override BindingContext here - the page sets the VM and the XAML uses x:Reference ThisView
    // BindingContext = this; <-- removed
  }

  private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
  {
    if (bindable is not MenuItemView view)
      return;

    if (oldValue is INotifyCollectionChanged oldCollection)
      oldCollection.CollectionChanged -= view.OnItemsCollectionChanged;

    if (newValue is INotifyCollectionChanged newCollection)
      newCollection.CollectionChanged += view.OnItemsCollectionChanged;

    view.RefreshDisplayItems();
  }

  private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    MainThread.BeginInvokeOnMainThread(RefreshDisplayItems);
  }

  private void RefreshDisplayItems()
  {
    DisplayItems.Clear();

    if (Items is null)
    {
      MainThread.BeginInvokeOnMainThread(() => ItemsCollection.ItemsSource = null);
      OnPropertyChanged(nameof(DisplayItems));
      System.Diagnostics.Debug.WriteLine("DisplayItems cleared (Items is null).");
      return;
    }

    foreach (var item in Items)
    {
      var quantity = _orderState.Lines
        .FirstOrDefault(x => x.MenuItemId == item.Id)?.Quantity ?? 0;

      DisplayItems.Add(new MenuItemCardViewModel
      {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Price = (decimal)item.Price,
        ImageUrl = item.ImageUrl,
        Quantity = quantity
      });
    }

    System.Diagnostics.Debug.WriteLine($"DisplayItems populated: {DisplayItems.Count}");

    MainThread.BeginInvokeOnMainThread(() =>
    {
      // Force rebind to ensure the CollectionView refreshes
      ItemsCollection.ItemsSource = null;
      ItemsCollection.ItemsSource = DisplayItems;
      System.Diagnostics.Debug.WriteLine($"ItemsCollection.ItemsSource set; DisplayItems count: {DisplayItems.Count}");
    });

    OnPropertyChanged(nameof(DisplayItems));
  }

  private void OnAddClicked(object? sender, EventArgs e)
  {
    if (sender is Button button &&
        button.CommandParameter is int menuItemId)
    {
      // find the display card to capture name/price snapshot
      var card = DisplayItems.FirstOrDefault(c => c.Id == menuItemId);
      var name = card?.Name ?? string.Empty;
      var unitPrice = (double)(card?.Price ?? 0m);

      _orderState.AddLine(menuItemId, name, unitPrice);
      RefreshDisplayItems();
    }
  }

  private void OnDecreaseClicked(object? sender, EventArgs e)
  {
    if (sender is Button button &&
        button.CommandParameter is int menuItemId)
    {
      // Use OrderState API instead of mutating lines directly
      _orderState.RemoveLine(menuItemId);
      RefreshDisplayItems();
    }
  }

  public new event PropertyChangedEventHandler? PropertyChanged;

  protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}

public class MenuItemCardViewModel
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public string ImageUrl { get; set; } = string.Empty;
  public int Quantity { get; set; }
  public bool HasQuantity => Quantity > 0;
  public string QuantityText => $"In order: {Quantity}";
}