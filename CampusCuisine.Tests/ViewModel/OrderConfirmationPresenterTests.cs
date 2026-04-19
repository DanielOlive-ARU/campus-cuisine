using CampusCuisine.Models;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class OrderConfirmationPresenterTests
{
  [Fact]
  public void FormatMessage_FullConfirmation_ReturnsAllLines()
  {
    var dto = new OrderConfirmationDto
    {
      Id = 42,
      Status = "received",
      TotalItems = 3,
      GrandTotal = 17.50,
      EstimatedPrepMinutes = 15
    };

    var result = OrderConfirmationPresenter.FormatMessage(dto);

    Assert.Equal(
      "Order ID: 42\n" +
      "Status: received\n" +
      "Total items: 3\n" +
      "Total: £17.50\n" +
      "Estimated preparation time: about 15 minutes",
      result);
  }

  [Fact]
  public void FormatMessage_MissingStatus_OmitsStatusLine()
  {
    var dto = new OrderConfirmationDto
    {
      Id = 1,
      Status = string.Empty,
      TotalItems = 2,
      GrandTotal = 5.00,
      EstimatedPrepMinutes = 10
    };

    var result = OrderConfirmationPresenter.FormatMessage(dto);

    Assert.Equal(
      "Order ID: 1\n" +
      "Total items: 2\n" +
      "Total: £5.00\n" +
      "Estimated preparation time: about 10 minutes",
      result);
  }

  [Fact]
  public void FormatMessage_WhitespaceStatus_OmitsStatusLine()
  {
    var dto = new OrderConfirmationDto
    {
      Id = 1,
      Status = "   ",
      TotalItems = 1,
      GrandTotal = 1.0,
      EstimatedPrepMinutes = null
    };

    var result = OrderConfirmationPresenter.FormatMessage(dto);

    Assert.DoesNotContain("Status:", result);
  }

  [Fact]
  public void FormatMessage_MissingPrepMinutes_OmitsPrepLine()
  {
    var dto = new OrderConfirmationDto
    {
      Id = 7,
      Status = "pending",
      TotalItems = 1,
      GrandTotal = 3.25,
      EstimatedPrepMinutes = null
    };

    var result = OrderConfirmationPresenter.FormatMessage(dto);

    Assert.Equal(
      "Order ID: 7\n" +
      "Status: pending\n" +
      "Total items: 1\n" +
      "Total: £3.25",
      result);
  }
}
