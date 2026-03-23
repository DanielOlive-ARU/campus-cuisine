using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class DessertsPage : ContentPage
{
  public DessertsPage()
  {
    InitializeComponent();
    BindingContext = new MenuItemViewModel();
  }
}