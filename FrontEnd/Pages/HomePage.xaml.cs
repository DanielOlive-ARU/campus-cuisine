using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  // Hardcoded featured menu item id for the MVP. A future iteration would
  // drive this from a backend "featured" flag or a date-based rotation.
  private const int FeaturedMenuItemId = 1;

  private readonly IOrderStateService _orderState;
  private readonly IApiService _api;
  private bool _featuredLoaded;

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

    // Fire-and-forget; LoadFeaturedAsync handles its own errors silently so
    // a backend outage does not surface a broken panel on the home page.
    _ = LoadFeaturedAsync();
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
    if (_featuredLoaded)
      return;

    try
    {
      var item = await _api.GetMenuItemAsync(FeaturedMenuItemId);
      if (item is null)
        return;

      MainThread.BeginInvokeOnMainThread(() =>
      {
        FeaturedName = item.Name;
        FeaturedDescription = item.Description;
        FeaturedPriceText = $"£{item.Price:F2}";
        FeaturedImageUrl = item.ImageUrl;
        FeaturedIsVisible = true;
        _featuredLoaded = true;
      });
    }
    catch
    {
      // Silent: if the backend is unreachable or the item is missing the
      // card stays hidden rather than showing a broken panel.
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
