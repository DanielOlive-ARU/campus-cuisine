using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class OrderSummaryPage : ContentPage, INotifyPropertyChanged
{
  private readonly IApiService _apiService;
  private readonly OrderState _orderState;

  public ObservableCollection<OrderSummaryLineViewModel> OrderItems { get; } = new();

  private string _resultMessage = string.Empty;
  public string ResultMessage
  {
    get => _resultMessage;
    set
    {
      if (_resultMessage != value)
      {
        _resultMessage = value;
        OnPropertyChanged();
      }
    }
  }

  private string _errorMessage = string.Empty;
  public string ErrorMessage
  {
    get => _errorMessage;
    set
    {
      if (_errorMessage != value)
      {
        _errorMessage = value;
        OnPropertyChanged();
      }
    }
  }

  public string TotalItemsText => $"Total items: {_orderState.TotalItems}";
  public string GrandTotalText => $"Grand total: £{OrderItems.Sum(x => x.LineTotal):F2}";

  public OrderSummaryPage()
  {
    InitializeComponent();

    _apiService = App.Services.GetRequiredService<IApiService>();
    _orderState = App.Services.GetRequiredService<OrderState>();

    BindingContext = this;
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    await LoadOrderItemsAsync();
  }

  private async Task LoadOrderItemsAsync()
  {
    OrderItems.Clear();
    ResultMessage = string.Empty;
    ErrorMessage = string.Empty;

    foreach (var line in _orderState.Lines)
    {
      var item = await _apiService.GetMenuItemAsync(line.MenuItemId);
      if (item != null)
      {
        OrderItems.Add(new OrderSummaryLineViewModel
        {
          MenuItemId = item.Id,
          Name = item.Name,
          Description = item.Description,
          UnitPrice = item.Price,
          Quantity = line.Quantity
        });
      }
    }

    RefreshTotals();
  }

  private void RefreshTotals()
  {
    OnPropertyChanged(nameof(TotalItemsText));
    OnPropertyChanged(nameof(GrandTotalText));
  }

  private async void OnIncreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is Button button && button.CommandParameter is int menuItemId)
    {
      var line = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == menuItemId);
      if (line != null)
      {
        line.Quantity++;
        await LoadOrderItemsAsync();
      }
    }
  }

  private async void OnDecreaseQuantityClicked(object? sender, EventArgs e)
  {
    if (sender is Button button && button.CommandParameter is int menuItemId)
    {
      var line = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == menuItemId);
      if (line == null)
        return;

      if (line.Quantity > 1)
      {
        line.Quantity--;
      }
      else
      {
        bool remove = await DisplayAlertAsync(
          "Remove Item",
          "Quantity is 1. Remove this item from the order?",
          "Yes",
          "No");

        if (!remove)
          return;

        _orderState.Lines.Remove(line);
      }

      await LoadOrderItemsAsync();
    }
  }

  private async void OnRemoveItemClicked(object? sender, EventArgs e)
  {
    if (sender is Button button && button.CommandParameter is int menuItemId)
    {
      var line = _orderState.Lines.FirstOrDefault(x => x.MenuItemId == menuItemId);
      if (line == null)
        return;

      bool confirm = await DisplayAlertAsync(
        "Remove Item",
        "Remove this item from your order?",
        "Yes",
        "No");

      if (!confirm)
        return;

      _orderState.Lines.Remove(line);
      await LoadOrderItemsAsync();
    }
  }

  private async void OnClearOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.Lines.Any())
    {
      await DisplayAlertAsync("Clear Order", "Your order is already empty.", "OK");
      return;
    }

    bool confirm = await DisplayAlertAsync(
      "Clear Order",
      "Are you sure you want to clear the entire order?",
      "Yes",
      "No");

    if (!confirm)
      return;

    _orderState.Clear();
    await LoadOrderItemsAsync();
  }

  private async void OnPlaceOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.Lines.Any())
    {
      await DisplayAlertAsync("Order", "Your order is empty.", "OK");
      return;
    }

    try
    {
      var request = _orderState.ToCreateOrderRequest();
      var result = await _apiService.PostOrderAsync(request);

      if (result == null)
      {
        await DisplayAlertAsync("Order Failed", "No response was returned from the server.", "OK");
        return;
      }

      var message =
        $"Order ID: {result.Id}\n" +
        $"Status: {result.Status}\n" +
        $"Total Items: {result.TotalItems}\n" +
        $"Grand Total: £{result.GrandTotal:F2}\n\n" +
        $"{result.Message}";

      await DisplayAlertAsync("Order Placed", message, "OK");

      _orderState.Clear();
      await LoadOrderItemsAsync();
    }
    catch (ApiException ex)
    {
      string title = ex.StatusCode switch
      {
        400 => "Order Error",
        404 => "Not Found",
        422 => "Validation Error",
        0 => "Network Error",
        _ => "Request Error"
      };

      await DisplayAlertAsync(title, ex.Message, "OK");
    }
    catch (Exception ex)
    {
      await DisplayAlertAsync("Unexpected Error", ex.Message, "OK");
    }
  }

  public new event PropertyChangedEventHandler? PropertyChanged;

  protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}

public class OrderSummaryLineViewModel
{
  public int MenuItemId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public decimal UnitPrice { get; set; }
  public int Quantity { get; set; }
  public decimal LineTotal => UnitPrice * Quantity;
}