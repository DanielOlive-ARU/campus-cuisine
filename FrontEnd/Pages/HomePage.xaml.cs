using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
  private readonly OrderState _orderState;

  public HomePage()
  {
    InitializeComponent();

    _orderState = App.Services.GetRequiredService<OrderState>();

    // Use this page as binding context for simple properties
    BindingContext = this;

    // Subscribe to changes on OrderState and its Lines collection
    _orderState.PropertyChanged += OrderState_PropertyChanged;
    if (_orderState.Lines is INotifyCollectionChanged coll)
      coll.CollectionChanged += Lines_CollectionChanged;

    // Ensure initial values are shown
    UpdateOrderInfo();
  }

  private void Lines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    UpdateOrderInfo();
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    // Recompute whenever TotalItems or Lines change
    if (string.IsNullOrEmpty(e?.PropertyName) || e.PropertyName == nameof(OrderState.TotalItems))
    {
      UpdateOrderInfo();
    }
  }

  private void UpdateOrderInfo()
  {
    // Compute on background but update UI on main thread
    var totalItems = _orderState.TotalItems;
    var grand = _orderState.Lines.Sum(x => x.Quantity * x.UnitPrice);
    var totalText = $"{totalItems} item{(totalItems == 1 ? "" : "s")}";
    var grandText = $"£{grand:F2}";

    MainThread.BeginInvokeOnMainThread(() =>
    {
      TotalItemsText = $"Items: {totalText}";
      GrandTotalText = $"Total: {grandText}";
      HasOrder = totalItems > 0;
    });
  }

  private string _totalItemsText = "Items: 0 items";
  public string TotalItemsText
  {
    get => _totalItemsText;
    set
    {
      if (_totalItemsText != value)
      {
        _totalItemsText = value;
        OnPropertyChanged();
      }
    }
  }

  private string _grandTotalText = "Total: £0.00";
  public string GrandTotalText
  {
    get => _grandTotalText;
    set
    {
      if (_grandTotalText != value)
      {
        _grandTotalText = value;
        OnPropertyChanged();
      }
    }
  }

  private bool _hasOrder;
  public bool HasOrder
  {
    get => _hasOrder;
    set
    {
      if (_hasOrder != value)
      {
        _hasOrder = value;
        OnPropertyChanged();
      }
    }
  }

  private async void OnStartNewOrderClicked(object sender, EventArgs e)
  {
    // Clear local order state and navigate to the default menu (Starters)
    _orderState.Clear();

    // Update UI immediately
    UpdateOrderInfo();

    await Shell.Current.GoToAsync("///StartersPage");
  }

  private async void OnContinueOrderClicked(object sender, EventArgs e)
  {
    // Only navigate if there is an order
    if (!_orderState.Lines.Any())
      return;

    await Shell.Current.GoToAsync(nameof(OrderSummaryPage));
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();

    // Unsubscribe to avoid leaks
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
    if (_orderState.Lines is INotifyCollectionChanged coll)
      coll.CollectionChanged -= Lines_CollectionChanged;
  }

  public new event PropertyChangedEventHandler? PropertyChanged;
  protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}