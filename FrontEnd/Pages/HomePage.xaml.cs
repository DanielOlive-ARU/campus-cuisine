using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  private readonly HomePageViewModel _vm;

  public HomePage(HomePageViewModel vm)
  {
    InitializeComponent();
    _vm = vm ?? throw new ArgumentNullException(nameof(vm));
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
