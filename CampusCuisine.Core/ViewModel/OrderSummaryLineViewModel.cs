using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;

namespace CampusCuisine.ViewModel
{
  public class OrderSummaryLineViewModel : INotifyPropertyChanged
  {
    private int _menuItemId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private double _unitPrice;
    private int _quantity;
    private string _quantityText = "0";

    public int MenuItemId
    {
      get => _menuItemId;
      set
      {
        if (_menuItemId != value)
        {
          _menuItemId = value;
          OnPropertyChanged();
        }
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          OnPropertyChanged();
        }
      }
    }

    public string Description
    {
      get => _description;
      set
      {
        if (_description != value)
        {
          _description = value;
          OnPropertyChanged();
        }
      }
    }

    public double UnitPrice
    {
      get => _unitPrice;
      set
      {
        if (_unitPrice != value)
        {
          _unitPrice = value;
          OnPropertyChanged();
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
          _quantityText = _quantity.ToString();
          OnPropertyChanged();
          OnPropertyChanged(nameof(QuantityText));
          OnPropertyChanged(nameof(LineTotal));
        }
      }
    }

    public string QuantityText
    {
      get => _quantityText;
      set
      {
        if (_quantityText != value)
        {
          _quantityText = value;
          OnPropertyChanged();
        }
      }
    }

    public double LineTotal => _unitPrice * _quantity;

    public OrderSummaryLineViewModel()
    {
    }

    public OrderSummaryLineViewModel(OrderLineEntry source)
    {
      UpdateFrom(source);
    }

    public void UpdateFrom(OrderLineEntry source)
    {
      MenuItemId = source.MenuItemId;
      Name = source.Name;
      Description = source.Description;
      UnitPrice = source.UnitPrice;
      Quantity = source.Quantity;
    }

    public static bool TryValidateQuantity(string? text, out int validated, out string? errorMessage)
    {
      validated = 0;
      errorMessage = null;

      var trimmed = text?.Trim();
      if (string.IsNullOrWhiteSpace(trimmed) || !int.TryParse(trimmed, out var parsed))
      {
        errorMessage = "Please enter a whole number quantity.";
        return false;
      }

      if (parsed <= 0)
      {
        errorMessage = "Quantity must be greater than zero.";
        return false;
      }

      if (parsed > 999)
      {
        errorMessage = "Quantity is too large.";
        return false;
      }

      validated = parsed;
      return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
