using System.ComponentModel;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Views;

public partial class OrderSummaryBar : ContentView
{
  private readonly IOrderStateService _orderState;
  private bool _isSubscribed;

  public OrderSummaryBar()
  {
    InitializeComponent();

    _orderState = App.Services.GetRequiredService<IOrderStateService>();

    Loaded += OnLoaded;
    Unloaded += OnUnloaded;

    RefreshSummary();
  }

  private void OnLoaded(object? sender, EventArgs e)
  {
    if (_isSubscribed)
      return;

    _orderState.PropertyChanged += OrderState_PropertyChanged;
    _isSubscribed = true;
    RefreshSummary();
  }

  private void OnUnloaded(object? sender, EventArgs e)
  {
    if (!_isSubscribed)
      return;

    _orderState.PropertyChanged -= OrderState_PropertyChanged;
    _isSubscribed = false;
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(IOrderStateService.TotalItems) ||
        e.PropertyName == nameof(IOrderStateService.GrandTotal) ||
        string.IsNullOrEmpty(e.PropertyName))
    {
      RefreshSummary();
    }
  }

  private void RefreshSummary()
  {
    MainThread.BeginInvokeOnMainThread(() =>
    {
      var totalItems = _orderState.TotalItems;
      OrderTotalLabel.Text = $"{totalItems} item{(totalItems == 1 ? string.Empty : "s")}";
      OrderGrandTotalLabel.Text = $"£{_orderState.GrandTotal:F2}";
    });
  }

  private async void OnOrderSummaryClicked(object? sender, EventArgs e)
  {
    await Shell.Current.GoToAsync("//OrderSummaryPage");
  }
}
