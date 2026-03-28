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

  public ObservableCollection<MenuItemModel> OrderItems { get; } = new();

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

  public OrderSummaryPage()
  {
    InitializeComponent();

    _apiService = Application.Current!.Services.GetRequiredService<IApiService>();
    _orderState = Application.Current!.Services.GetRequiredService<OrderState>();

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
        item.Description = $"{item.Description} (Qty: {line.Quantity})";
        OrderItems.Add(item);
      }
    }

    OnPropertyChanged(nameof(TotalItemsText));
  }

  private async void OnPlaceOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.Lines.Any())
    {
      await DisplayAlert("Order", "Your order is empty.", "OK");
      return;
    }

    try
    {
      var request = _orderState.ToCreateOrderRequest();
      var result = await _apiService.PostOrderAsync(request);

      if (result == null)
      {
        await DisplayAlert("Order Failed", "No response was returned from the server.", "OK");
        return;
      }

      var message =
        $"Order ID: {result.Id}\n" +
        $"Status: {result.Status}\n" +
        $"Total Items: {result.TotalItems}\n" +
        $"Grand Total: £{result.GrandTotal:F2}\n\n" +
        $"{result.Message}";

      await DisplayAlert("Order Placed", message, "OK");

      _orderState.Clear();
      OrderItems.Clear();
      OnPropertyChanged(nameof(TotalItemsText));
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

      await DisplayAlert(title, ex.Message, "OK");
    }
    catch (Exception ex)
    {
      await DisplayAlert("Unexpected Error", ex.Message, "OK");
    }
  }

  public new event PropertyChangedEventHandler? PropertyChanged;

  protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}