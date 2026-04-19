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
    private readonly Dictionary<OrderLineDto, OrderSummaryLineViewModel> _map = new();
    private readonly INotifyCollectionChanged? _linesNotifier;
    private bool _disposed;

    public OrderSummaryLineSync(IOrderStateService orderState, ObservableCollection<OrderSummaryLineViewModel> target)
    {
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));
      _target = target ?? throw new ArgumentNullException(nameof(target));

      foreach (var dto in _orderState.Lines)
      {
        SubscribeAndAdd(dto, _target.Count);
      }

      _linesNotifier = _orderState.Lines as INotifyCollectionChanged;
      if (_linesNotifier != null)
      {
        _linesNotifier.CollectionChanged += OnLinesCollectionChanged;
      }
    }

    private void SubscribeAndAdd(OrderLineDto dto, int index)
    {
      var vm = new OrderSummaryLineViewModel(dto);
      if (index < 0 || index > _target.Count)
      {
        _target.Add(vm);
      }
      else
      {
        _target.Insert(index, vm);
      }
      _map[dto] = vm;
      dto.PropertyChanged += OnLinePropertyChanged;
    }

    private void UnsubscribeAndRemove(OrderLineDto dto)
    {
      dto.PropertyChanged -= OnLinePropertyChanged;
      if (_map.TryGetValue(dto, out var vm))
      {
        _target.Remove(vm);
        _map.Remove(dto);
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
            foreach (OrderLineDto dto in e.NewItems)
            {
              SubscribeAndAdd(dto, index);
              if (index >= 0) index++;
            }
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if (e.OldItems != null)
          {
            foreach (OrderLineDto dto in e.OldItems)
            {
              UnsubscribeAndRemove(dto);
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          // ObservableCollection.Clear() raises Reset without OldItems,
          // so we rely on our own map to unsubscribe cleanly.
          foreach (var dto in _map.Keys)
          {
            dto.PropertyChanged -= OnLinePropertyChanged;
          }
          _map.Clear();
          _target.Clear();

          foreach (var dto in _orderState.Lines)
          {
            SubscribeAndAdd(dto, _target.Count);
          }
          break;

        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
          break;
      }
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender is not OrderLineDto dto) return;
      if (!_map.TryGetValue(dto, out var vm)) return;

      switch (e.PropertyName)
      {
        case nameof(OrderLineDto.Quantity):
          if (vm.Quantity != dto.Quantity) vm.Quantity = dto.Quantity;
          break;
        case nameof(OrderLineDto.UnitPrice):
          if (vm.UnitPrice != dto.UnitPrice) vm.UnitPrice = dto.UnitPrice;
          break;
        case nameof(OrderLineDto.Name):
          if (vm.Name != dto.Name) vm.Name = dto.Name;
          break;
        case nameof(OrderLineDto.Description):
          if (vm.Description != dto.Description) vm.Description = dto.Description;
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

      foreach (var dto in _map.Keys)
      {
        dto.PropertyChanged -= OnLinePropertyChanged;
      }
      _map.Clear();
    }
  }
}
