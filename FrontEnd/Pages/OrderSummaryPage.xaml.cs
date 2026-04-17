using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace CampusCuisine.Pages;

public partial class OrderSummaryPage : ContentPage
{
  private readonly IOrderStateService _orderState;
  private readonly IApiService _api;
  private bool _isPlacingOrder;

  public ObservableCollection<OrderLineDto> Lines => _orderState.Lines;

  public string TotalItemsText => $"Total items: {_orderState.TotalItems}";

  public string GrandTotalText => $"Grand total: £{_orderState.GrandTotal:F2}";

  public OrderSummaryPage()
  {
    InitializeComponent();
    _orderState = App.Services.GetRequiredService<IOrderStateService>();
    _api = App.Services.GetRequiredService<IApiService>();
    BindingContext = this;
    SetPlaceOrderBusy(false);
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    _orderState.PropertyChanged -= OnOrderStatePropertyChanged;
    _orderState.PropertyChanged += OnOrderStatePropertyChanged;
    RefreshTotals();
  }

  protected override void OnDisappearing()
  {
    _orderState.PropertyChanged -= OnOrderStatePropertyChanged;
    base.OnDisappearing();
  }

  private void OnOrderStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
    RefreshTotals();
  }

  private void RefreshTotals()
  {
    OnPropertyChanged(nameof(TotalItemsText));
    OnPropertyChanged(nameof(GrandTotalText));
  }

  private static int? GetMenuItemId(object? commandParameter)
  {
    return commandParameter switch
    {
      int i => i,
      string s when int.TryParse(s, out var pi) => pi,
      _ => null
    };
  }

  private bool TryGetLineFromEntry(Entry entry, out OrderLineDto? line)
  {
    line = entry.BindingContext as OrderLineDto;
    return line is not null;
  }

  private async Task HandleQuantityEntryAsync(Entry entry, bool showValidationAlerts)
  {
    if (!TryGetLineFromEntry(entry, out var line) || line is null)
      return;

    var currentQuantity = line.Quantity;
    var text = line.QuantityText?.Trim();

    if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out var parsedQuantity))
    {
      line.QuantityText = currentQuantity.ToString();
      if (showValidationAlerts)
        await DisplayAlertAsync("Invalid Quantity", "Please enter a whole number quantity.", "OK");
      return;
    }

    if (parsedQuantity <= 0)
    {
      line.QuantityText = currentQuantity.ToString();
      if (showValidationAlerts)
        await DisplayAlertAsync("Invalid Quantity", "Quantity must be greater than zero.", "OK");
      return;
    }

    if (parsedQuantity > 999)
    {
      line.QuantityText = currentQuantity.ToString();
      if (showValidationAlerts)
        await DisplayAlertAsync("Invalid Quantity", "Quantity is too large.", "OK");
      return;
    }

    _orderState.SetQuantity(line.MenuItemId, parsedQuantity);
  }

  private async void OnQuantityEntryCompleted(object? sender, EventArgs e)
  {
    if (sender is Entry entry)
      await HandleQuantityEntryAsync(entry, showValidationAlerts: true);
  }

  private async void OnQuantityEntryUnfocused(object? sender, FocusEventArgs e)
  {
    if (sender is Entry entry)
      await HandleQuantityEntryAsync(entry, showValidationAlerts: false);
  }

  private async void OnDecreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = GetMenuItemId(b.CommandParameter);

    if (!id.HasValue)
      return;

    var existing = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == id.Value);
    if (existing is null)
      return;

    if (existing.Quantity > 1)
    {
      _orderState.SetQuantity(id.Value, existing.Quantity - 1);
      return;
    }

    var confirm = await DisplayAlertAsync("Remove Item", $"Remove '{existing.Name}' from your order?", "Remove", "Cancel");
    if (confirm)
      _orderState.RemoveLine(id.Value, existing.Quantity);
  }

  private void OnIncreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = GetMenuItemId(b.CommandParameter);

    if (!id.HasValue)
      return;

    // capture existing snapshot if available
    var existing = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == id.Value);
    if (existing is null)
      return;

    _orderState.SetQuantity(id.Value, existing.Quantity + 1);
  }

  private async void OnRemoveItemClicked(object? sender, EventArgs e)
  {
    if (sender is not Button b)
      return;

    int? id = GetMenuItemId(b.CommandParameter);

    if (!id.HasValue)
      return;

    var existing = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == id.Value);
    if (existing != null)
    {
      var confirm = await DisplayAlertAsync("Remove Item", $"Remove '{existing.Name}' from your order?", "Remove", "Cancel");
      if (confirm)
        _orderState.RemoveLine(id.Value, existing.Quantity);
    }
  }

  private async void OnClearOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.HasOrder)
      return;

    var confirm = await DisplayAlertAsync("Clear Order", "Clear all items from your order?", "Clear", "Cancel");
    if (confirm)
      _orderState.Clear();
  }

  private async void OnPlaceOrderClicked(object? sender, EventArgs e)
  {
    if (_isPlacingOrder)
      return;

    if (!_orderState.HasOrder)
    {
      await DisplayAlertAsync("Order Empty", "Your order is empty. Please add an item before placing an order.", "OK");
      return;
    }

    var request = _orderState.ToCreateOrderRequest();
    SetPlaceOrderBusy(true);

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
    finally
    {
      SetPlaceOrderBusy(false);
    }
  }

  private void SetPlaceOrderBusy(bool isBusy)
  {
    _isPlacingOrder = isBusy;
    if (PlaceOrderButton is not null)
    {
      PlaceOrderButton.IsEnabled = !isBusy;
      PlaceOrderButton.Text = isBusy ? "Placing Order..." : "Place Order";
    }
  }
}
