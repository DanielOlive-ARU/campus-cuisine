using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace CampusCuisine.Pages;

public partial class HomePage : ContentPage
{
  private readonly OrderState _orderState;

  public HomePage()
  {
    InitializeComponent();

    _orderState = App.Services.GetRequiredService<OrderState>();

    BindingContext = this;
    UpdateOrderInfo();
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    _orderState.PropertyChanged -= OrderState_PropertyChanged;
    _orderState.PropertyChanged += OrderState_PropertyChanged;
    UpdateOrderInfo();
  }

  private void OrderState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (string.IsNullOrEmpty(e?.PropertyName)
        || e.PropertyName == nameof(OrderState.TotalItems)
        || e.PropertyName == nameof(OrderState.GrandTotal)
        || e.PropertyName == nameof(OrderState.HasOrder))
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
      TotalItemsText = $"Items: {totalText}";
      GrandTotalText = $"Total: {grandText}";
      HasOrder = totalItems > 0;
    });
  }

  private string _totalItemsText = "Items: 0 items";
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

  private string _grandTotalText = "Total: £0.00";
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

  private async void OnStartNewOrderClicked(object sender, EventArgs e)
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

  private async void OnContinueOrderClicked(object sender, EventArgs e)
  {
    if (!_orderState.HasOrder)
      return;

    await Shell.Current.GoToAsync("//OrderSummaryPage");
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();

    _orderState.PropertyChanged -= OrderState_PropertyChanged;
  }
}
