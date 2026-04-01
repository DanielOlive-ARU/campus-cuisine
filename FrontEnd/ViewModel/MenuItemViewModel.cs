using CampusCuisine.Models;
using CampusCuisine.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Http;

namespace CampusCuisine.ViewModel
{
  public class MenuItemViewModel : INotifyPropertyChanged
  {
    private readonly IApiService _apiService;
    private string _category = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

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

    public string ErrorMessage
    {
      get => _errorMessage;
      set
      {
        if (_errorMessage != value)
        {
          _errorMessage = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(HasError));
        }
      }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

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
        ErrorMessage = string.Empty;
        MenuItems.Clear();

        var backendCategory = MapCategory(Category);
        var items = await _apiService.GetMenuByCategoryAsync(backendCategory);

        foreach (var item in items)
        {
          MenuItems.Add(item);
        }
      }
      catch (HttpRequestException)
      {
        MenuItems.Clear();
        ErrorMessage = "The menu service is currently unavailable.";
      }
      catch (TaskCanceledException)
      {
        MenuItems.Clear();
        ErrorMessage = "The menu request timed out.";
      }
      catch (Exception)
      {
        MenuItems.Clear();
        ErrorMessage = "Unable to load menu items right now.";
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