using CampusCuisine.Models;
using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  private readonly IOrderStateService _orderState;
  private readonly IApiService _api;
  private List<MenuItemModel>? _cachedMains;
  private List<MenuItemModel>? _cachedDesserts;

  public HomePage()
  {
    InitializeComponent();

    _orderState = App.Services.GetRequiredService<IOrderStateService>();
    _api = App.Services.GetRequiredService<IApiService>();

    BindingContext = this;
    UpdateOrderInfo();
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
    _orderState.PropertyChanged += OrderState_PropertyChanged;
    UpdateOrderInfo();

    // Fire-and-forget; both loaders handle their own errors silently so
    // a backend outage does not surface broken panels on the home page.
    _ = LoadFeaturedAsync();
    _ = LoadIndulgenceAsync();
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (string.IsNullOrEmpty(e?.PropertyName)
        || e.PropertyName == nameof(IOrderStateService.TotalItems)
        || e.PropertyName == nameof(IOrderStateService.GrandTotal)
        || e.PropertyName == nameof(IOrderStateService.HasOrder))
    {
      UpdateOrderInfo();
    }
  }

  private void UpdateOrderInfo()
  {
    var totalItems = _orderState.TotalItems;
    var grand = _orderState.GrandTotal;
    var totalText = $"{totalItems} item{(totalItems == 1 ? "" : "s")}";
    var grandText = $"£{grand:F2}";

    MainThread.BeginInvokeOnMainThread(() =>
    {
      TotalItemsText = totalText;
      GrandTotalText = grandText;
      HasOrder = totalItems > 0;
    });
  }

  private string _totalItemsText = "0 items";
  public string TotalItemsText
  {
    get => _totalItemsText;
    set
    {
      if (_totalItemsText != value)
      {
        _totalItemsText = value;
        OnPropertyChanged();
      }
    }
  }

  private string _grandTotalText = "£0.00";
  public string GrandTotalText
  {
    get => _grandTotalText;
    set
    {
      if (_grandTotalText != value)
      {
        _grandTotalText = value;
        OnPropertyChanged();
      }
    }
  }

  private bool _hasOrder;
  public bool HasOrder
  {
    get => _hasOrder;
    set
    {
      if (_hasOrder != value)
      {
        _hasOrder = value;
        OnPropertyChanged();
      }
    }
  }

  private bool _featuredIsVisible;
  public bool FeaturedIsVisible
  {
    get => _featuredIsVisible;
    set
    {
      if (_featuredIsVisible != value)
      {
        _featuredIsVisible = value;
        OnPropertyChanged();
      }
    }
  }

  private string _featuredName = string.Empty;
  public string FeaturedName
  {
    get => _featuredName;
    set
    {
      if (_featuredName != value)
      {
        _featuredName = value;
        OnPropertyChanged();
      }
    }
  }

  private string _featuredDescription = string.Empty;
  public string FeaturedDescription
  {
    get => _featuredDescription;
    set
    {
      if (_featuredDescription != value)
      {
        _featuredDescription = value;
        OnPropertyChanged();
      }
    }
  }

  private string _featuredPriceText = string.Empty;
  public string FeaturedPriceText
  {
    get => _featuredPriceText;
    set
    {
      if (_featuredPriceText != value)
      {
        _featuredPriceText = value;
        OnPropertyChanged();
      }
    }
  }

  private string _featuredImageUrl = string.Empty;
  public string FeaturedImageUrl
  {
    get => _featuredImageUrl;
    set
    {
      if (_featuredImageUrl != value)
      {
        _featuredImageUrl = value;
        OnPropertyChanged();
      }
    }
  }

  private async Task LoadFeaturedAsync()
  {
    // Same pattern as LoadIndulgenceAsync: cache the category list once per
    // session (no repeat network calls) but re-roll the random pick on
    // every appearance so visits to Home keep surfacing variety.
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

      MainThread.BeginInvokeOnMainThread(() =>
      {
        FeaturedName = pick.Name;
        FeaturedDescription = pick.Description;
        FeaturedPriceText = $"£{pick.Price:F2}";
        FeaturedImageUrl = pick.ImageUrl;
        FeaturedIsVisible = true;
      });
    }
    catch
    {
      // Silent: if the backend is unreachable or no mains are available
      // the card stays hidden rather than showing a broken panel.
    }
  }

  private string _indulgenceName = string.Empty;
  public string IndulgenceName
  {
    get => _indulgenceName;
    set
    {
      if (_indulgenceName != value)
      {
        _indulgenceName = value;
        OnPropertyChanged();
      }
    }
  }

  private string _indulgenceDescription = string.Empty;
  public string IndulgenceDescription
  {
    get => _indulgenceDescription;
    set
    {
      if (_indulgenceDescription != value)
      {
        _indulgenceDescription = value;
        OnPropertyChanged();
      }
    }
  }

  private string _indulgencePriceText = string.Empty;
  public string IndulgencePriceText
  {
    get => _indulgencePriceText;
    set
    {
      if (_indulgencePriceText != value)
      {
        _indulgencePriceText = value;
        OnPropertyChanged();
      }
    }
  }

  private string _indulgenceImageUrl = string.Empty;
  public string IndulgenceImageUrl
  {
    get => _indulgenceImageUrl;
    set
    {
      if (_indulgenceImageUrl != value)
      {
        _indulgenceImageUrl = value;
        OnPropertyChanged();
      }
    }
  }

  private bool _indulgenceIsVisible;
  public bool IndulgenceIsVisible
  {
    get => _indulgenceIsVisible;
    set
    {
      if (_indulgenceIsVisible != value)
      {
        _indulgenceIsVisible = value;
        OnPropertyChanged();
      }
    }
  }

  private async Task LoadIndulgenceAsync()
  {
    // Cache the dessert list once per session (no repeat network calls)
    // but re-roll the random pick on every appearance so visits to Home
    // keep surfacing variety — the cache does not interfere with
    // randomness because the cache holds the list, not the chosen item.
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

      MainThread.BeginInvokeOnMainThread(() =>
      {
        IndulgenceName = pick.Name;
        IndulgenceDescription = pick.Description;
        IndulgencePriceText = $"£{pick.Price:F2}";
        IndulgenceImageUrl = pick.ImageUrl;
        IndulgenceIsVisible = true;
      });
    }
    catch
    {
      // Silent: keep the card hidden if desserts cannot be loaded.
    }
  }

  private async void OnStartNewOrderClicked(object? sender, EventArgs e)
  {
    if (_orderState.HasOrder)
    {
      var confirm = await DisplayAlertAsync(
        "Start New Order",
        "Start a new order? This will clear your current order.",
        "Start New",
        "Cancel");

      if (!confirm)
        return;

      _orderState.Clear();
      UpdateOrderInfo();
    }

    await Shell.Current.GoToAsync("///StartersPage");
  }

  private async void OnContinueOrderClicked(object? sender, EventArgs e)
  {
    if (!_orderState.HasOrder)
      return;

    await Shell.Current.GoToAsync("//OrderSummaryPage");
  }

  private async void OnQuickNavigateClicked(object? sender, EventArgs e)
  {
    if (sender is not Button button ||
        button.CommandParameter is not string route ||
        string.IsNullOrWhiteSpace(route))
      return;

    await Shell.Current.GoToAsync(route);
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();

    _orderState.PropertyChanged -= OrderState_PropertyChanged;
  }
}
