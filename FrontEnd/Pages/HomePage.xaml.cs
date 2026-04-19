using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  private readonly IOrderStateService _orderState;
  private readonly HomePageViewModel _vm;

  public HomePage()
  {
    InitializeComponent();

    var api = App.Services.GetRequiredService<IApiService>();
    _orderState = App.Services.GetRequiredService<IOrderStateService>();
    _vm = new HomePageViewModel(api, _orderState);

    BindingContext = _vm;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();

    // Fire-and-forget; the view-model silently hides cards on failure so
    // a backend outage does not surface broken panels on the home page.
    _ = _vm.InitializeAsync();
  }

  private async void OnStartNewOrderClicked(object? sender, EventArgs e)
  {
    if (_orderState.HasOrder)
    {
      var confirm = await DisplayAlertAsync(
        "Start New Order",
        "Start a new order? This will clear your current order.",
        "Start New",
        "Cancel");

      if (!confirm)
        return;

      _orderState.Clear();
    }

    await Shell.Current.GoToAsync("///StartersPage");
  }

  private async void OnContinueOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.HasOrder)
      return;

    await Shell.Current.GoToAsync("//OrderSummaryPage");
  }

  private async void OnQuickNavigateClicked(object? sender, EventArgs e)
  {
    if (sender is not Button button ||
        button.CommandParameter is not string route ||
        string.IsNullOrWhiteSpace(route))
      return;

    await Shell.Current.GoToAsync(route);
  }
}
