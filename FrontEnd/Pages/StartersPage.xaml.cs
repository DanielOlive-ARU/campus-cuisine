using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class StartersPage : ContentPage
{
  public StartersPage()
  {
    InitializeComponent();
    BindingContext = new MenuItemViewModel();
  }
}