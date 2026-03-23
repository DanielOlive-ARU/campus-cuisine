using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.ViewModel;

namespace CampusCuisine.Pages;

public partial class MainsPage : ContentPage
{
  public MainsPage()
  {
    InitializeComponent();
    BindingContext = new MenuItemViewModel();
  }
}