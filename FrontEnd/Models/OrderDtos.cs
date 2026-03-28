using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CampusCuisine.Models;

public class OrderLineDto : INotifyPropertyChanged
{
  private int _menuItemId;
  private int _quantity;

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
      }
    }
  }

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
