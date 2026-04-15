using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CampusCuisine.Pages;

public partial class DessertsPage : ContentPage
{
  private readonly OrderState _orderState;

  public DessertsPage()
  {
    InitializeComponent();

    var api = App.Services.GetRequiredService<IApiService>();
    BindingContext = new MenuItemViewModel(api, "Desserts");

    _orderState = App.Services.GetRequiredService<OrderState>();
    RefreshOrderSummary();
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(OrderState.TotalItems) ||
        e.PropertyName == nameof(OrderState.GrandTotal) ||
        string.IsNullOrEmpty(e.PropertyName))
    {
      RefreshOrderSummary();
    }
  }

  private void RefreshOrderSummary()
  {
    MainThread.BeginInvokeOnMainThread(() =>
    {
      var totalItems = _orderState.TotalItems;
      OrderTotalLabel.Text = $"{totalItems} item{(totalItems == 1 ? string.Empty : "s")}";
      OrderGrandTotalLabel.Text = $"£{_orderState.GrandTotal:F2}";
    });
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
    _orderState.PropertyChanged += OrderState_PropertyChanged;
    RefreshOrderSummary();

    if (BindingContext is MenuItemViewModel vm)
    {
      try
      {
        await vm.InitializeAsync();
      }
      catch
      {
        await DisplayAlertAsync("Menu Unavailable", "Failed to load desserts. Please try again later.", "OK");
      }
    }
  }

  private async void OnOrderSummaryClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync("//OrderSummaryPage");
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
  }
}
