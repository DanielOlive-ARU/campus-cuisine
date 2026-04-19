using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public class OrderSummaryPageViewModel : INotifyPropertyChanged, IDisposable
  {
    private readonly IOrderStateService _orderState;
    private readonly IDialogService? _dialogService;
    private OrderSummaryLineSync? _sync;
    private bool _disposed;

    public ObservableCollection<OrderSummaryLineViewModel> Lines { get; } = new();

    public string TotalItemsText => $"Total items: {_orderState.TotalItems}";

    public string GrandTotalText => FormattableString.Invariant($"Grand total: £{_orderState.GrandTotal:F2}");

    public bool HasOrder => _orderState.HasOrder;

    public OrderSummaryPageViewModel(IOrderStateService orderState)
      : this(orderState, dialogService: null)
    {
    }

    public OrderSummaryPageViewModel(IOrderStateService orderState, IDialogService? dialogService)
    {
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));
      _dialogService = dialogService;
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
