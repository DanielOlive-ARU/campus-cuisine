using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace CampusCuisine;

public partial class DessertsPage : ContentPage
{
  public DessertsPage()
  {
    InitializeComponent();

    var api = Application.Current!.Services.GetRequiredService<IApiService>();
    BindingContext = new MenuItemViewModel(api, "Desserts");
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