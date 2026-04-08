using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CampusCuisine.Pages;

public partial class MainsPage : ContentPage
{
  private readonly OrderState _orderState;

  public MainsPage()
  {
    InitializeComponent();

    var api = App.Services.GetRequiredService<IApiService>();
    BindingContext = new MenuItemViewModel(api, "Mains");

    _orderState = App.Services.GetRequiredService<OrderState>();
    OrderTotalLabel.Text = $"{_orderState.TotalItems} items";
    _orderState.PropertyChanged += OrderState_PropertyChanged;
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(OrderState.TotalItems) || string.IsNullOrEmpty(e.PropertyName))
    {
      MainThread.BeginInvokeOnMainThread(() =>
      {
        OrderTotalLabel.Text = $"{_orderState.TotalItems} items";
      });
    }
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();

    if (BindingContext is MenuItemViewModel vm)
    {
      try
      {
        await vm.InitializeAsync();
      }
      catch
      {
        await DisplayAlertAsync("Menu Unavailable", "Failed to load mains. Please try again later.", "OK");
      }
    }
  }

  private async void OnOrderSummaryClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync(nameof(OrderSummaryPage));
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
  }
}