using CampusCuisine.Services;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class MainsPage : ContentPage
{
  public MainsPage(IApiService api)
  {
    InitializeComponent();

    BindingContext = new MenuItemViewModel(api, "Mains");
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
}
