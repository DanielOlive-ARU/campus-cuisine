using CampusCuisine.Models;
using CampusCuisine.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CampusCuisine.ViewModel
{
  public class MenuItemViewModel : INotifyPropertyChanged
  {
    private readonly IApiService _apiService;
    private string _category = string.Empty;
    private bool _isBusy;

    public ObservableCollection<MenuItemModel> MenuItems { get; } = new();

    public string Category
    {
      get => _category;
      set
      {
        if (_category != value)
        {
          _category = value;
          OnPropertyChanged();
        }
      }
    }

    public bool IsBusy
    {
      get => _isBusy;
      set
      {
        if (_isBusy != value)
        {
          _isBusy = value;
          OnPropertyChanged();
        }
      }
    }

    public MenuItemViewModel(IApiService apiService, string category)
    {
      _apiService = apiService;
      Category = category;
    }

    public async Task InitializeAsync()
    {
      if (IsBusy)
        return;

      try
      {
        IsBusy = true;
        MenuItems.Clear();

        var backendCategory = MapCategory(Category);
        var items = await _apiService.GetMenuByCategoryAsync(backendCategory);

        foreach (var item in items)
        {
          MenuItems.Add(item);
        }
      }
      finally
      {
        IsBusy = false;
      }
    }

    private string MapCategory(string category)
    {
      return category switch
      {
        "Starters" => "appetizer",
        "Mains" => "main",
        "Desserts" => "dessert",
        _ => category.ToLowerInvariant()
      };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
