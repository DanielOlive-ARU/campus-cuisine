using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public sealed class MenuItemCardSync : IDisposable
  {
    private readonly ObservableCollection<MenuItemModel> _items;
    private readonly IOrderStateService _orderState;
    private readonly ObservableCollection<MenuItemCardViewModel> _target;
    private readonly Dictionary<int, MenuItemCardViewModel> _vmByMenuItemId = new();
    private readonly HashSet<OrderLineEntry> _subscribedEntries = new();
    private readonly INotifyCollectionChanged? _itemsNotifier;
    private readonly INotifyCollectionChanged? _linesNotifier;
    private bool _disposed;

    public MenuItemCardSync(
      ObservableCollection<MenuItemModel> items,
      IOrderStateService orderState,
      ObservableCollection<MenuItemCardViewModel> target)
    {
      _items = items ?? throw new ArgumentNullException(nameof(items));
      _orderState = orderState ?? throw new ArgumentNullException(nameof(orderState));
      _target = target ?? throw new ArgumentNullException(nameof(target));

      foreach (var item in _items)
      {
        AddVmForItem(item);
      }

      _itemsNotifier = _items as INotifyCollectionChanged;
      if (_itemsNotifier != null)
      {
        _itemsNotifier.CollectionChanged += OnItemsCollectionChanged;
      }

      _linesNotifier = _orderState.Lines as INotifyCollectionChanged;
      if (_linesNotifier != null)
      {
        _linesNotifier.CollectionChanged += OnLinesCollectionChanged;
      }

      foreach (var entry in _orderState.Lines)
      {
        SubscribeToEntry(entry);
      }
    }

    private void AddVmForItem(MenuItemModel item)
    {
      var currentQuantity = _orderState.GetQuantityForMenuItem(item.Id);
      var vm = new MenuItemCardViewModel(item, currentQuantity);
      _target.Add(vm);
      _vmByMenuItemId[item.Id] = vm;
    }

    private void RemoveVmForItem(MenuItemModel item)
    {
      if (_vmByMenuItemId.TryGetValue(item.Id, out var vm))
      {
        _target.Remove(vm);
        _vmByMenuItemId.Remove(item.Id);
      }
    }

    private void SubscribeToEntry(OrderLineEntry entry)
    {
      if (_subscribedEntries.Add(entry))
      {
        entry.PropertyChanged += OnEntryPropertyChanged;
      }
    }

    private void UnsubscribeFromEntry(OrderLineEntry entry)
    {
      if (_subscribedEntries.Remove(entry))
      {
        entry.PropertyChanged -= OnEntryPropertyChanged;
      }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          if (e.NewItems != null)
          {
            foreach (MenuItemModel item in e.NewItems)
              AddVmForItem(item);
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if (e.OldItems != null)
          {
            foreach (MenuItemModel item in e.OldItems)
              RemoveVmForItem(item);
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          _vmByMenuItemId.Clear();
          _target.Clear();
          foreach (var item in _items)
          {
            AddVmForItem(item);
          }
          break;

        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Move:
          break;
      }
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          if (e.NewItems != null)
          {
            foreach (OrderLineEntry entry in e.NewItems)
            {
              SubscribeToEntry(entry);
              if (_vmByMenuItemId.TryGetValue(entry.MenuItemId, out var vm))
                vm.Quantity = entry.Quantity;
            }
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          if (e.OldItems != null)
          {
            foreach (OrderLineEntry entry in e.OldItems)
            {
              UnsubscribeFromEntry(entry);
              if (_vmByMenuItemId.TryGetValue(entry.MenuItemId, out var vm))
                vm.Quantity = 0;
            }
          }
          break;

        case NotifyCollectionChangedAction.Reset:
          foreach (var entry in _subscribedEntries)
          {
            entry.PropertyChanged -= OnEntryPropertyChanged;
          }
          _subscribedEntries.Clear();

          foreach (var vm in _vmByMenuItemId.Values)
          {
            vm.Quantity = 0;
          }

          foreach (var entry in _orderState.Lines)
          {
            SubscribeToEntry(entry);
            if (_vmByMenuItemId.TryGetValue(entry.MenuItemId, out var vm))
              vm.Quantity = entry.Quantity;
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
      if (!_vmByMenuItemId.TryGetValue(entry.MenuItemId, out var vm)) return;

      if (e.PropertyName == nameof(OrderLineEntry.Quantity))
      {
        if (vm.Quantity != entry.Quantity)
          vm.Quantity = entry.Quantity;
      }
    }

    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;

      if (_itemsNotifier != null)
        _itemsNotifier.CollectionChanged -= OnItemsCollectionChanged;

      if (_linesNotifier != null)
        _linesNotifier.CollectionChanged -= OnLinesCollectionChanged;

      foreach (var entry in _subscribedEntries)
        entry.PropertyChanged -= OnEntryPropertyChanged;
      _subscribedEntries.Clear();
    }
  }
}
