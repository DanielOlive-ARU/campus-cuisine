using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public class OrderState : INotifyPropertyChanged
{
  private readonly ObservableCollection<OrderLineDto> _lines = new();

  public ObservableCollection<OrderLineDto> Lines => _lines;

  public int TotalItems => _lines.Sum(x => x.Quantity);

  public void AddLine(int menuItemId, int quantity = 1)
  {
    if (quantity <= 0)
      return;

    var existing = _lines.FirstOrDefault(x => x.MenuItemId == menuItemId);

    if (existing is null)
    {
      _lines.Add(new OrderLineDto
      {
        MenuItemId = menuItemId,
        Quantity = quantity
      });
    }
    else
    {
      existing.Quantity += quantity;
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
    {
      _lines.Remove(existing);
    }

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
      }).ToList()
    };
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
