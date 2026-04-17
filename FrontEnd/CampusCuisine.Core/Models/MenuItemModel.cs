using System.Text.Json.Serialization;

namespace CampusCuisine.Models;

public class MenuItemModel
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string Description { get; set; } = string.Empty;

  [JsonPropertyName("category")]
  public string Category { get; set; } = string.Empty;

  // numeric price (matches backend float)
  [JsonPropertyName("price")]
  public decimal Price { get; set; }

  // backend field is `image_url`
  [JsonPropertyName("image_url")]
  public string ImageUrl { get; set; } = string.Empty;

  [JsonPropertyName("is_available")]
  public bool IsAvailable { get; set; }
}