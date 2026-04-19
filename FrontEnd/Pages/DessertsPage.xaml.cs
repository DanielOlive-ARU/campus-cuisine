using CampusCuisine.Services;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class DessertsPage : ContentPage
{
  public DessertsPage(IApiService api)
  {
    InitializeComponent();

    BindingContext = new MenuItemViewModel(api, "Desserts");
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
        await DisplayAlertAsync("Menu Unavailable", "Failed to load desserts. Please try again later.", "OK");
      }
    }
  }
}
