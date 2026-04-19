using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public class OrderSummaryPageViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly IOrderStateService _orderState;
    private readonly IApiService? _api;
    private readonly IDialogService? _dialogService;
    private readonly INavigationService? _navigationService;
    private OrderSummaryLineSync? _sync;
    private bool _disposed;
    private bool _isPlacingOrder;

    public ObservableCollection<OrderSummaryLineViewModel> Lines { get; } = new();

    public string TotalItemsText => $"Total items: {_orderState.TotalItems}";

    public string GrandTotalText => FormattableString.Invariant($"Grand total: £{_orderState.GrandTotal:F2}");

    public bool HasOrder => _orderState.HasOrder;

    public bool IsPlacingOrder => _isPlacingOrder;

    public string PlaceOrderButtonText => _isPlacingOrder ? "Placing Order..." : "Place Order";

    public ICommand ClearOrderCommand { get; }

    public ICommand PlaceOrderCommand { get; }

    public OrderSummaryPageViewModel(IOrderStateService orderState)
      : this(orderState, api: null, dialogService: null, navigationService: null)
    {
    }

    public OrderSummaryPageViewModel(IOrderStateService orderState, IDialogService? dialogService)
      : this(orderState, api: null, dialogService: dialogService, navigationService: null)
    {
    }

    public OrderSummaryPageViewModel(
      IOrderStateService orderState,
      IApiService? api,
      IDialogService? dialogService,
      INavigationService? navigationService)
    {
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));
      _api = api;
      _dialogService = dialogService;
      _navigationService = navigationService;

      ClearOrderCommand = new AsyncRelayCommand(ClearOrderAsync);
      PlaceOrderCommand = new AsyncRelayCommand(PlaceOrderAsync);
    }

    public void Attach()
    {
      if (_disposed)
        throw new ObjectDisposedException(nameof(OrderSummaryPageViewModel));

      // Idempotent: if the caller re-attaches without detaching first,
      // tear down the previous wiring cleanly before re-seeding.
      _orderState.PropertyChanged -= OnOrderStatePropertyChanged;
      _sync?.Dispose();
      _sync = null;
      Lines.Clear();

      _sync = new OrderSummaryLineSync(_orderState, Lines, _dialogService);
      _orderState.PropertyChanged += OnOrderStatePropertyChanged;

      NotifyTotals();
    }

    public void Detach()
    {
      _orderState.PropertyChanged -= OnOrderStatePropertyChanged;
      _sync?.Dispose();
      _sync = null;
    }

    private async Task ClearOrderAsync()
    {
      if (!_orderState.HasOrder)
        return;

      var confirm = _dialogService is null
        ? true
        : await _dialogService.ConfirmAsync(
            "Clear Order",
            "Clear all items from your order?",
            "Clear",
            "Cancel");

      if (confirm)
        _orderState.Clear();
    }

    private async Task PlaceOrderAsync()
    {
      if (!_orderState.HasOrder)
      {
        if (_dialogService is not null)
          await _dialogService.ShowAsync(
            "Order Empty",
            "Your order is empty. Please add an item before placing an order.",
            "OK");
        return;
      }

      if (_api is null)
        return;

      SetPlacingOrder(true);

      try
      {
        var request = _orderState.ToCreateOrderRequest();
        var confirmation = await _api.PostOrderAsync(request);

        if (confirmation is null)
        {
          if (_dialogService is not null)
            await _dialogService.ShowAsync(
              "Order Failed",
              "Server returned an error placing your order.",
              "OK");
          return;
        }

        if (_dialogService is not null)
          await _dialogService.ShowAsync(
            "Order Confirmed",
            OrderConfirmationPresenter.FormatMessage(confirmation),
            "OK");

        _orderState.Clear();

        if (_navigationService is not null)
          await _navigationService.GoToAsync("..");
      }
      catch (Exception ex)
      {
        if (_dialogService is not null)
          await _dialogService.ShowAsync("Network Error", ex.Message, "OK");
      }
      finally
      {
        SetPlacingOrder(false);
      }
    }

    private void SetPlacingOrder(bool isBusy)
    {
      if (_isPlacingOrder == isBusy)
        return;

      _isPlacingOrder = isBusy;
      OnPropertyChanged(nameof(IsPlacingOrder));
      OnPropertyChanged(nameof(PlaceOrderButtonText));
    }

    private void OnOrderStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      NotifyTotals();
    }

    private void NotifyTotals()
    {
      OnPropertyChanged(nameof(TotalItemsText));
      OnPropertyChanged(nameof(GrandTotalText));
      OnPropertyChanged(nameof(HasOrder));
    }

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;
      Detach();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
