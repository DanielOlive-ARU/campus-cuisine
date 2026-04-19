using CampusCuisine.Services;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class StartersPage : ContentPage
{
  public StartersPage(IApiService api)
  {
    InitializeComponent();

    BindingContext = new MenuItemViewModel(api, "Starters");
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
        await DisplayAlertAsync("Menu Unavailable", "Failed to load starters. Please try again later.", "OK");
      }
    }
  }
}
