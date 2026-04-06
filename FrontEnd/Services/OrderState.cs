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

  public void AddLine(int menuItemId, string? name = null, double unitPrice = 0, int quantity = 1)
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
        UnitPrice = unitPrice
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
    }

    OnPropertyChanged(nameof(Lines));
    OnPropertyChanged(nameof(TotalItems));
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

    OnPropertyChanged(nameof(Lines));
    OnPropertyChanged(nameof(TotalItems));
  }

  public void Clear()
  {
    _lines.Clear();
    OnPropertyChanged(nameof(Lines));
    OnPropertyChanged(nameof(TotalItems));
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

  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}