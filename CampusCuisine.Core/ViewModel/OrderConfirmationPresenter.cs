using CampusCuisine.Models;

namespace CampusCuisine.ViewModel
{
  public static class OrderConfirmationPresenter
  {
    public static string FormatMessage(OrderConfirmationDto confirmation)
    {
      var lines = new List<string>
      {
        $"Order ID: {confirmation.Id}"
      };

      if (!string.IsNullOrWhiteSpace(confirmation.Status))
      {
        lines.Add($"Status: {confirmation.Status}");
      }

      lines.Add($"Total items: {confirmation.TotalItems}");
      lines.Add(FormattableString.Invariant($"Total: £{confirmation.GrandTotal:F2}"));

      if (confirmation.EstimatedPrepMinutes.HasValue)
      {
        lines.Add($"Estimated preparation time: about {confirmation.EstimatedPrepMinutes.Value} minutes");
      }

      return string.Join('\n', lines);
    }
  }
}
