using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CampusCuisine.Models;

public class OrderLineEntry : INotifyPropertyChanged
{
  private int _quantity;
  private MenuItemSnapshot _snapshot;

  public int MenuItemId { get; }

  public OrderLineEntry(int menuItemId, MenuItemSnapshot snapshot, int quantity)
  {
    MenuItemId = menuItemId;
    _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    _quantity = quantity;
  }

  public MenuItemSnapshot Snapshot
  {
    get => _snapshot;
    set
    {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      if (!_snapshot.Equals(value))
      {
        _snapshot = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(UnitPrice));
        OnPropertyChanged(nameof(LineTotal));
      }
    }
  }

  public int Quantity
  {
    get => _quantity;
    set
    {
      if (_quantity != value)
      {
        _quantity = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(LineTotal));
      }
    }
  }

  public string Name => _snapshot.Name;
  public string Description => _snapshot.Description;
  public double UnitPrice => _snapshot.UnitPrice;
  public double LineTotal => _snapshot.UnitPrice * _quantity;

  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
