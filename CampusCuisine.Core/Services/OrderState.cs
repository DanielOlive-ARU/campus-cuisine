using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public class OrderState : IOrderStateService
{
  private readonly ObservableCollection<OrderLineEntry> _lines = new();

  public ObservableCollection<OrderLineEntry> Lines => _lines;

  public int TotalItems => _lines.Sum(x => x.Quantity);

  public double GrandTotal => _lines.Sum(x => x.LineTotal);

  public bool HasOrder => _lines.Any();

  public OrderState()
  {
    _lines.CollectionChanged += (_, e) =>
    {
      if (e.NewItems is not null)
      {
        foreach (OrderLineEntry entry in e.NewItems)
          entry.PropertyChanged += OnEntryPropertyChanged;
      }

      if (e.OldItems is not null)
      {
        foreach (OrderLineEntry entry in e.OldItems)
          entry.PropertyChanged -= OnEntryPropertyChanged;
      }

      NotifyStateChanged();
    };
  }

  public void AddLine(int menuItemId, string? name = null, double unitPrice = 0, int quantity = 1, string? description = null)
  {
    if (quantity <= 0)
      return;

    var existing = _lines.FirstOrDefault(x => x.MenuItemId == menuItemId);

    if (existing is null)
    {
      var snapshot = new MenuItemSnapshot(
        name ?? string.Empty,
        description ?? string.Empty,
        unitPrice);
      var entry = new OrderLineEntry(menuItemId, snapshot, quantity);
      _lines.Add(entry);
    }
    else
    {
      existing.Quantity += quantity;

      // Fill-if-empty upgrade: if an earlier AddLine created the entry
      // with stub snapshot data, a later AddLine with real data upgrades it.
      var current = existing.Snapshot;
      var newName = string.IsNullOrWhiteSpace(current.Name) && !string.IsNullOrWhiteSpace(name)
        ? name!
        : current.Name;
      var newDescription = string.IsNullOrWhiteSpace(current.Description) && !string.IsNullOrWhiteSpace(description)
        ? description!
        : current.Description;
      var newPrice = current.UnitPrice == 0 && unitPrice > 0
        ? unitPrice
        : current.UnitPrice;

      if (newName != current.Name || newDescription != current.Description || newPrice != current.UnitPrice)
      {
        existing.Snapshot = new MenuItemSnapshot(newName, newDescription, newPrice);
      }
    }

    NotifyStateChanged();
  }

  public void RemoveLine(int menuItemId, int quantity = 1)
  {
    if (quantity <= 0)
      return;

    var existing = _lines.FirstOrDefault(x => x.MenuItemId == menuItemId);
    if (existing is null)
      return;

    existing.Quantity -= quantity;
    if (existing.Quantity <= 0)
      _lines.Remove(existing);

    NotifyStateChanged();
  }

  public void SetQuantity(int menuItemId, int quantity)
  {
    var existing = _lines.FirstOrDefault(x => x.MenuItemId == menuItemId);

    if (existing is null)
      return;

    if (quantity <= 0)
    {
      _lines.Remove(existing);
      NotifyStateChanged();
      return;
    }

    existing.Quantity = quantity;
    NotifyStateChanged();
  }

  public void Clear()
  {
    _lines.Clear();
    NotifyStateChanged();
  }

  public int GetQuantityForMenuItem(int menuItemId)
  {
    return _lines.FirstOrDefault(x => x.MenuItemId == menuItemId)?.Quantity ?? 0;
  }

  public CreateOrderRequestDto ToCreateOrderRequest()
  {
    return new CreateOrderRequestDto
    {
      Items = _lines.Select(x => new OrderLineDto
      {
        MenuItemId = x.MenuItemId,
        Quantity = x.Quantity
      }).ToList()
    };
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is nameof(OrderLineEntry.Quantity) or nameof(OrderLineEntry.LineTotal))
      NotifyStateChanged();
  }

  private void NotifyStateChanged()
  {
    OnPropertyChanged(nameof(Lines));
    OnPropertyChanged(nameof(TotalItems));
    OnPropertyChanged(nameof(GrandTotal));
    OnPropertyChanged(nameof(HasOrder));
  }

  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
