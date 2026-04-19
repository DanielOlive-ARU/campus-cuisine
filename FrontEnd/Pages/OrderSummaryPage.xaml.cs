using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class OrderSummaryPage : ContentPage
{
  private readonly IOrderStateService _orderState;
  private readonly IApiService _api;
  private readonly OrderSummaryPageViewModel _vm;
  private bool _isPlacingOrder;

  public OrderSummaryPage()
  {
    InitializeComponent();
    _orderState = App.Services.GetRequiredService<IOrderStateService>();
    _api = App.Services.GetRequiredService<IApiService>();
    _vm = new OrderSummaryPageViewModel(_orderState);
    BindingContext = _vm;
    SetPlaceOrderBusy(false);
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    _vm.Attach();
  }

  protected override void OnDisappearing()
  {
    _vm.Detach();
    base.OnDisappearing();
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

  private async Task HandleQuantityEntryAsync(Entry entry, bool showValidationAlerts)
  {
    if (entry.BindingContext is not OrderSummaryLineViewModel vm)
      return;

    var current = vm.Quantity;

    if (!OrderSummaryLineViewModel.TryValidateQuantity(vm.QuantityText, out var validated, out var errorMessage))
    {
      vm.QuantityText = current.ToString();
      if (showValidationAlerts && errorMessage is not null)
        await DisplayAlertAsync("Invalid Quantity", errorMessage, "OK");
      return;
    }

    _orderState.SetQuantity(vm.MenuItemId, validated);
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

    if (PlaceOrderButton is not null)
    {
      await PlaceOrderButton.ScaleToAsync(0.96, 80);
      await PlaceOrderButton.ScaleToAsync(1.0, 80);
    }

    SetPlaceOrderBusy(true);

    try
    {
      var confirmation = await _api.PostOrderAsync(request);
      if (confirmation is null)
      {
        await DisplayAlertAsync("Order Failed", "Server returned an error placing your order.", "OK");
        return;
      }

      await DisplayAlertAsync("Order Confirmed", OrderConfirmationPresenter.FormatMessage(confirmation), "OK");
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
