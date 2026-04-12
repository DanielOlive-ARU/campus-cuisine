using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CampusCuisine.Models;

public class OrderLineDto : INotifyPropertyChanged
{
  private int _menuItemId;
  private int _quantity;
  private string _name = string.Empty;
  private string _description = string.Empty;
  private double _unitPrice;

  [JsonPropertyName("menu_item_id")]
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

  [JsonPropertyName("quantity")]
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

  // Local snapshot fields used by the frontend only:
  [JsonIgnore]
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

  [JsonIgnore]
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

  [JsonIgnore]
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

  [JsonIgnore]
  public double LineTotal => UnitPrice * Quantity;

  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}

public class CreateOrderRequestDto
{
  [JsonPropertyName("items")]
  public List<OrderLineDto> Items { get; set; } = new();
}

public class OrderConfirmationDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; } = string.Empty;

  [JsonPropertyName("total_items")]
  public int TotalItems { get; set; }

  [JsonPropertyName("grand_total")]
  public double GrandTotal { get; set; }

  [JsonPropertyName("message")]
  public string Message { get; set; } = string.Empty;
}

public class OrderReadDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; } = string.Empty;

  [JsonPropertyName("total_items")]
  public int TotalItems { get; set; }

  [JsonPropertyName("grand_total")]
  public double GrandTotal { get; set; }
}
