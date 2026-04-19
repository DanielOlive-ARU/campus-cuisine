using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class OrderSummaryPage : ContentPage
{
  private readonly IOrderStateService _orderState;
  private readonly OrderSummaryPageViewModel _vm;

  public OrderSummaryPage()
  {
    InitializeComponent();

    _orderState = App.Services.GetRequiredService<IOrderStateService>();
    var api = App.Services.GetRequiredService<IApiService>();
    var dialogService = App.Services.GetRequiredService<IDialogService>();
    var navigationService = App.Services.GetRequiredService<INavigationService>();

    _vm = new OrderSummaryPageViewModel(_orderState, api, dialogService, navigationService);
    BindingContext = _vm;
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

  // Cosmetic press-feedback animation. The button's Command handles the
  // order-placement logic independently; Clicked fires alongside Command
  // on button tap so the animation plays while the API call begins.
  private async void OnPlaceOrderClicked(object? sender, EventArgs e)
  {
    if (PlaceOrderButton is null)
      return;

    await PlaceOrderButton.ScaleToAsync(0.96, 80);
    await PlaceOrderButton.ScaleToAsync(1.0, 80);
  }
}
