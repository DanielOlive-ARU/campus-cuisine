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

  public OrderSummaryPage()
  {
    InitializeComponent();
    _orderState = App.Services.GetRequiredService<OrderState>();
    _api = App.Services.GetRequiredService<IApiService>();
    BindingContext = this;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    // ensure UI bound collection is current
    SummaryCollection.ItemsSource = Lines;
  }

  private void OnIncreaseClicked(object sender, EventArgs e)
  {
    if (sender is Button b && b.CommandParameter is int id)
    {
      var line = Lines.FirstOrDefault(x => x.MenuItemId == id);
      _orderState.AddLine(id, line?.Name, line?.UnitPrice ?? 0);
    }
  }

  private void OnDecreaseClicked(object sender, EventArgs e)
  {
    if (sender is Button b && b.CommandParameter is int id)
    {
      _orderState.RemoveLine(id);
    }
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