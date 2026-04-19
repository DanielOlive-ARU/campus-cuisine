using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public class HomePageViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly IApiService _api;
    private readonly IOrderStateService _orderState;

    private List<MenuItemModel>? _cachedMains;
    private List<MenuItemModel>? _cachedDesserts;

    private string _totalItemsText = "0 items";
    private string _grandTotalText = "£0.00";
    private bool _hasOrder;

    private bool _featuredIsVisible;
    private string _featuredName = string.Empty;
    private string _featuredDescription = string.Empty;
    private string _featuredPriceText = string.Empty;
    private string _featuredImageUrl = string.Empty;

    private bool _indulgenceIsVisible;
    private string _indulgenceName = string.Empty;
    private string _indulgenceDescription = string.Empty;
    private string _indulgencePriceText = string.Empty;
    private string _indulgenceImageUrl = string.Empty;

    private bool _disposed;

    public HomePageViewModel(IApiService api, IOrderStateService orderState)
    {
      _api = api ?? throw new ArgumentNullException(nameof(api));
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));

      _orderState.PropertyChanged += OnOrderStatePropertyChanged;
      UpdateOrderTotals();
    }

    public string TotalItemsText
    {
      get => _totalItemsText;
      private set
      {
        if (_totalItemsText != value)
        {
          _totalItemsText = value;
          OnPropertyChanged();
        }
      }
    }

    public string GrandTotalText
    {
      get => _grandTotalText;
      private set
      {
        if (_grandTotalText != value)
        {
          _grandTotalText = value;
          OnPropertyChanged();
        }
      }
    }

    public bool HasOrder
    {
      get => _hasOrder;
      private set
      {
        if (_hasOrder != value)
        {
          _hasOrder = value;
          OnPropertyChanged();
        }
      }
    }

    public bool FeaturedIsVisible
    {
      get => _featuredIsVisible;
      private set
      {
        if (_featuredIsVisible != value)
        {
          _featuredIsVisible = value;
          OnPropertyChanged();
        }
      }
    }

    public string FeaturedName
    {
      get => _featuredName;
      private set
      {
        if (_featuredName != value)
        {
          _featuredName = value;
          OnPropertyChanged();
        }
      }
    }

    public string FeaturedDescription
    {
      get => _featuredDescription;
      private set
      {
        if (_featuredDescription != value)
        {
          _featuredDescription = value;
          OnPropertyChanged();
        }
      }
    }

    public string FeaturedPriceText
    {
      get => _featuredPriceText;
      private set
      {
        if (_featuredPriceText != value)
        {
          _featuredPriceText = value;
          OnPropertyChanged();
        }
      }
    }

    public string FeaturedImageUrl
    {
      get => _featuredImageUrl;
      private set
      {
        if (_featuredImageUrl != value)
        {
          _featuredImageUrl = value;
          OnPropertyChanged();
        }
      }
    }

    public bool IndulgenceIsVisible
    {
      get => _indulgenceIsVisible;
      private set
      {
        if (_indulgenceIsVisible != value)
        {
          _indulgenceIsVisible = value;
          OnPropertyChanged();
        }
      }
    }

    public string IndulgenceName
    {
      get => _indulgenceName;
      private set
      {
        if (_indulgenceName != value)
        {
          _indulgenceName = value;
          OnPropertyChanged();
        }
      }
    }

    public string IndulgenceDescription
    {
      get => _indulgenceDescription;
      private set
      {
        if (_indulgenceDescription != value)
        {
          _indulgenceDescription = value;
          OnPropertyChanged();
        }
      }
    }

    public string IndulgencePriceText
    {
      get => _indulgencePriceText;
      private set
      {
        if (_indulgencePriceText != value)
        {
          _indulgencePriceText = value;
          OnPropertyChanged();
        }
      }
    }

    public string IndulgenceImageUrl
    {
      get => _indulgenceImageUrl;
      private set
      {
        if (_indulgenceImageUrl != value)
        {
          _indulgenceImageUrl = value;
          OnPropertyChanged();
        }
      }
    }

    public async Task InitializeAsync()
    {
      await Task.WhenAll(LoadFeaturedAsync(), LoadIndulgenceAsync());
    }

    private async Task LoadFeaturedAsync()
    {
      try
      {
        if (_cachedMains is null)
        {
          var mains = await _api.GetMenuByCategoryAsync("main");
          if (mains is null || mains.Count == 0)
            return;

          _cachedMains = mains;
        }

        var pick = _cachedMains[Random.Shared.Next(_cachedMains.Count)];

        FeaturedName = pick.Name;
        FeaturedDescription = pick.Description;
        FeaturedPriceText = FormattableString.Invariant($"£{pick.Price:F2}");
        FeaturedImageUrl = pick.ImageUrl;
        FeaturedIsVisible = true;
      }
      catch
      {
        // Silent: keep the card hidden if mains cannot be loaded.
      }
    }

    private async Task LoadIndulgenceAsync()
    {
      try
      {
        if (_cachedDesserts is null)
        {
          var desserts = await _api.GetMenuByCategoryAsync("dessert");
          if (desserts is null || desserts.Count == 0)
            return;

          _cachedDesserts = desserts;
        }

        var pick = _cachedDesserts[Random.Shared.Next(_cachedDesserts.Count)];

        IndulgenceName = pick.Name;
        IndulgenceDescription = pick.Description;
        IndulgencePriceText = FormattableString.Invariant($"£{pick.Price:F2}");
        IndulgenceImageUrl = pick.ImageUrl;
        IndulgenceIsVisible = true;
      }
      catch
      {
        // Silent: keep the card hidden if desserts cannot be loaded.
      }
    }

    private void OnOrderStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(e?.PropertyName)
          || e.PropertyName == nameof(IOrderStateService.TotalItems)
          || e.PropertyName == nameof(IOrderStateService.GrandTotal)
          || e.PropertyName == nameof(IOrderStateService.HasOrder))
      {
        UpdateOrderTotals();
      }
    }

    private void UpdateOrderTotals()
    {
      var totalItems = _orderState.TotalItems;
      var grand = _orderState.GrandTotal;

      TotalItemsText = $"{totalItems} item{(totalItems == 1 ? "" : "s")}";
      GrandTotalText = FormattableString.Invariant($"£{grand:F2}");
      HasOrder = totalItems > 0;
    }

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;

      _orderState.PropertyChanged -= OnOrderStatePropertyChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
