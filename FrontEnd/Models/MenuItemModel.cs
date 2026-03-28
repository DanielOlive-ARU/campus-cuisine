using System.Text.Json.Serialization;

namespace CampusCuisine.Models;

public class MenuItemModel
{
  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string Category { get; set; } = string.Empty;

  public double Price { get; set; }

  [JsonPropertyName("image_url")]
  public string ImageUrl { get; set; } = string.Empty;

  [JsonPropertyName("is_available")]
  public bool IsAvailable { get; set; }
}