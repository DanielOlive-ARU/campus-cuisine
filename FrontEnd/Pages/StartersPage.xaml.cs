using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine.Pages;

public partial class StartersPage : ContentPage
{
  public StartersPage()
  {
    InitializeComponent();

    var api = App.Services.GetRequiredService<IApiService>();
    BindingContext = new MenuItemViewModel(api, "Starters");
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();

    if (BindingContext is MenuItemViewModel vm)
    {
      await vm.InitializeAsync();
    }
  }
}