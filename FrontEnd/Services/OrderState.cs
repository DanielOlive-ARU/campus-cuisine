using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public class OrderState : INotifyPropertyChanged
{
  private readonly ObservableCollection<OrderLineDto> _lines = new();

  public ObservableCollection<OrderLineDto> Lines => _lines;

  public int TotalItems => _lines.Sum(x => x.Quantity);

  public double GrandTotal => _lines.Sum(x => x.LineTotal);

  public bool HasOrder => _lines.Any();

  public OrderState()
  {
    _lines.CollectionChanged += (_, e) =>
    {
      if (e.NewItems is not null)
      {
        foreach (OrderLineDto line in e.NewItems)
          line.PropertyChanged += OnLinePropertyChanged;
      }

      if (e.OldItems is not null)
      {
        foreach (OrderLineDto line in e.OldItems)
          line.PropertyChanged -= OnLinePropertyChanged;
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
      var line = new OrderLineDto
      {
        MenuItemId = menuItemId,
        Quantity = quantity,
        Name = name ?? string.Empty,
        UnitPrice = unitPrice,
        Description = description ?? string.Empty
      };
      _lines.Add(line);
    }
    else
    {
      existing.Quantity += quantity;

      // If name/price were not set before, fill them
      if (string.IsNullOrWhiteSpace(existing.Name) && !string.IsNullOrWhiteSpace(name))
        existing.Name = name!;
      if (existing.UnitPrice == 0 && unitPrice > 0)
        existing.UnitPrice = unitPrice;
      if (string.IsNullOrWhiteSpace(existing.Description) && !string.IsNullOrWhiteSpace(description))
        existing.Description = description!;
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
        // Name and UnitPrice intentionally not copied — server expects only menu_item_id and quantity
      }).ToList()
    };
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is nameof(OrderLineDto.Quantity) or nameof(OrderLineDto.UnitPrice) or nameof(OrderLineDto.LineTotal))
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