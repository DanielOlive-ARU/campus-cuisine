using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public sealed class OrderSummaryLineSync : IDisposable
  {
    private readonly IOrderStateService _orderState;
    private readonly ObservableCollection<OrderSummaryLineViewModel> _target;
    private readonly Dictionary<OrderLineEntry, OrderSummaryLineViewModel> _map = new();
    private readonly INotifyCollectionChanged? _linesNotifier;
    private bool _disposed;

    public OrderSummaryLineSync(IOrderStateService orderState, ObservableCollection<OrderSummaryLineViewModel> target)
    {
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));
      _target = target ?? throw new ArgumentNullException(nameof(target));

      foreach (var entry in _orderState.Lines)
      {
        SubscribeAndAdd(entry, _target.Count);
      }

      _linesNotifier = _orderState.Lines as INotifyCollectionChanged;
      if (_linesNotifier != null)
      {
        _linesNotifier.CollectionChanged += OnLinesCollectionChanged;
      }
    }

    private void SubscribeAndAdd(OrderLineEntry entry, int index)
    {
      var vm = new OrderSummaryLineViewModel(entry);
      if (index < 0 || index > _target.Count)
      {
        _target.Add(vm);
      }
      else
      {
        _target.Insert(index, vm);
      }
      _map[entry] = vm;
      entry.PropertyChanged += OnEntryPropertyChanged;
    }

    private void UnsubscribeAndRemove(OrderLineEntry entry)
    {
      entry.PropertyChanged -= OnEntryPropertyChanged;
      if (_map.TryGetValue(entry, out var vm))
      {
        _target.Remove(vm);
        _map.Remove(entry);
      }
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          if (e.NewItems != null)
          {
            var index = e.NewStartingIndex;
            foreach (OrderLineEntry entry in e.NewItems)
            {
              SubscribeAndAdd(entry, index);
              if (index >= 0) index++;
            }
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if (e.OldItems != null)
          {
            foreach (OrderLineEntry entry in e.OldItems)
            {
              UnsubscribeAndRemove(entry);
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          // ObservableCollection.Clear() raises Reset without OldItems,
          // so we rely on our own map to unsubscribe cleanly.
          foreach (var entry in _map.Keys)
          {
            entry.PropertyChanged -= OnEntryPropertyChanged;
          }
          _map.Clear();
          _target.Clear();

          foreach (var entry in _orderState.Lines)
          {
            SubscribeAndAdd(entry, _target.Count);
          }
          break;

        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
          break;
      }
    }

    private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender is not OrderLineEntry entry) return;
      if (!_map.TryGetValue(entry, out var vm)) return;

      switch (e.PropertyName)
      {
        case nameof(OrderLineEntry.Quantity):
          if (vm.Quantity != entry.Quantity) vm.Quantity = entry.Quantity;
          break;
        case nameof(OrderLineEntry.UnitPrice):
          if (vm.UnitPrice != entry.UnitPrice) vm.UnitPrice = entry.UnitPrice;
          break;
        case nameof(OrderLineEntry.Name):
          if (vm.Name != entry.Name) vm.Name = entry.Name;
          break;
        case nameof(OrderLineEntry.Description):
          if (vm.Description != entry.Description) vm.Description = entry.Description;
          break;
      }
    }

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;

      if (_linesNotifier != null)
      {
        _linesNotifier.CollectionChanged -= OnLinesCollectionChanged;
      }

      foreach (var entry in _map.Keys)
      {
        entry.PropertyChanged -= OnEntryPropertyChanged;
      }
      _map.Clear();
    }
  }
}
