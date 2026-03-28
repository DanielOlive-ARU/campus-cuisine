using System.Text.Json.Serialization;

namespace CampusCuisine.Models;

public class OrderLineDto
{
  [JsonPropertyName("menu_item_id")]
  public int MenuItemId { get; set; }

  [JsonPropertyName("quantity")]
  public int Quantity { get; set; }
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
