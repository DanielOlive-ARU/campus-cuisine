using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace CampusCuisine.Pages;

public partial class OrderSummaryPage : ContentPage
{
  private readonly OrderState _orderState;
  private readonly IApiService _api;

  public ObservableCollection<OrderLineDto> Lines => _orderState.Lines;

  public string TotalItemsText => $"Total items: {_orderState.TotalItems}";

  public string GrandTotalText => $"Grand total: £{_orderState.GrandTotal:F2}";

  public OrderSummaryPage()
  {
    InitializeComponent();
    _orderState = App.Services.GetRequiredService<OrderState>();
    _api = App.Services.GetRequiredService<IApiService>();
    _orderState.PropertyChanged += (_, _) =>
    {
      OnPropertyChanged(nameof(TotalItemsText));
      OnPropertyChanged(nameof(GrandTotalText));
    };
    BindingContext = this;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    OnPropertyChanged(nameof(TotalItemsText));
    OnPropertyChanged(nameof(GrandTotalText));
  }

  private void OnDecreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = b.CommandParameter switch
    {
      int i => i,
      string s when int.TryParse(s, out var pi) => pi,
      _ => null
    };

    if (!id.HasValue)
      return;

    // decrease by one
    _orderState.RemoveLine(id.Value, 1);
  }

  private void OnIncreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = b.CommandParameter switch
    {
      int i => i,
      string s when int.TryParse(s, out var pi) => pi,
      _ => null
    };

    if (!id.HasValue)
      return;

    // capture existing snapshot if available
    var existing = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == id.Value);
    _orderState.AddLine(id.Value, existing?.Name, existing?.UnitPrice ?? 0, 1, existing?.Description);
  }

  private void OnRemoveItemClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = b.CommandParameter switch
    {
      int i => i,
      string s when int.TryParse(s, out var pi) => pi,
      _ => null
    };

    if (!id.HasValue)
      return;

    var existing = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == id.Value);
    if (existing != null)
    {
      // remove entire line
      _orderState.RemoveLine(id.Value, existing.Quantity);
    }
  }

  private void OnClearOrderClicked(object? sender, EventArgs e)
  {
    // Use the shared OrderState to clear the local order
    _orderState.Clear();

    // Optionally update any UI or navigate back
    // MainThread.BeginInvokeOnMainThread(() => { /* update UI if needed */ });
  }

  private async void OnPlaceOrderClicked(object sender, EventArgs e)
  {
    var request = _orderState.ToCreateOrderRequest();

    try
    {
      var confirmation = await _api.PostOrderAsync(request);
      if (confirmation is null)
      {
        await DisplayAlertAsync("Order Failed", "Server returned an error placing your order.", "OK");
        return;
      }

      await DisplayAlertAsync("Order Confirmed", $"Order ID: {confirmation.Id}\nTotal items: {confirmation.TotalItems}\nTotal: £{confirmation.GrandTotal:F2}", "OK");
      _orderState.Clear();
      await Shell.Current.GoToAsync("..");
    }
    catch (Exception ex)
    {
      await DisplayAlertAsync("Network Error", ex.Message, "OK");
    }
  }
}