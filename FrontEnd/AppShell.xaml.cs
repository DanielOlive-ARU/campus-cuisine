using CampusCuisine.Pages;

namespace CampusCuisine;

public partial class AppShell : Shell
{
  public AppShell()
  {
    InitializeComponent();

    // Register app routes used with Shell.Current.GoToAsync(...)
    Routing.RegisterRoute(nameof(OrderSummaryPage), typeof(OrderSummaryPage));
  }
}
