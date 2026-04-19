using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  private readonly HomePageViewModel _vm;

  public HomePage()
  {
    InitializeComponent();

    var api = App.Services.GetRequiredService<IApiService>();
    var orderState = App.Services.GetRequiredService<IOrderStateService>();
    var dialogService = App.Services.GetRequiredService<IDialogService>();
    var navigationService = App.Services.GetRequiredService<INavigationService>();

    _vm = new HomePageViewModel(api, orderState, dialogService, navigationService);

    BindingContext = _vm;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();

    // Fire-and-forget; the view-model silently hides cards on failure so
    // a backend outage does not surface broken panels on the home page.
    _ = _vm.InitializeAsync();
  }
}
