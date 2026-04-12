using CampusCuisine.Services;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  public HomePage()
  {
    InitializeComponent();
  }

  private void OnStartNewOrderClicked(object sender, EventArgs e)
  {
    var state = App.Services.GetRequiredService<OrderState>();
    state.Clear();
    Shell.Current.GoToAsync(nameof(StartersPage));
  }

  private void OnContinueOrderClicked(object sender, EventArgs e)
  {
    Shell.Current.GoToAsync(nameof(OrderSummaryPage));
  }
}