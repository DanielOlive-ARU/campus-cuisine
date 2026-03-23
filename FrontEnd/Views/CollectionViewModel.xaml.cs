using CampusCuisine.ViewModel;

namespace CampusCuisine.Models;

public partial class CollectionViewModel : ContentPage
{
	public CollectionViewModel()
	{
    InitializeComponent();
		this.BindingContext = new MenuItemViewModel();
  }
}