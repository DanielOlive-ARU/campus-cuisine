using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.Tests.TestDoubles;
using Xunit;

namespace CampusCuisine.Tests.Services;

public class ApiServiceTests
{
  [Fact]
  public async Task GetMenuByCategoryAsync_RelativeImageUrls_AreMadeAbsolute()
  {
    var service = CreateService(_ => JsonResponse(HttpStatusCode.OK, new[]
    {
      new MenuItemModel { Id = 1, Name = "Burger", ImageUrl = "images/burger.jpg" }
    }));

    var items = await service.GetMenuByCategoryAsync("main");

    var item = Assert.Single(items);
    Assert.Equal("http://localhost:8000/images/burger.jpg", item.ImageUrl);
  }

  [Fact]
  public async Task GetMenuByCategoryAsync_AbsoluteImageUrls_AreLeftUnchanged()
  {
    var service = CreateService(_ => JsonResponse(HttpStatusCode.OK, new[]
    {
      new MenuItemModel { Id = 1, Name = "Burger", ImageUrl = "https://cdn.example.com/burger.jpg" }
    }));

    var items = await service.GetMenuByCategoryAsync("main");

    var item = Assert.Single(items);
    Assert.Equal("https://cdn.example.com/burger.jpg", item.ImageUrl);
  }

  [Fact]
  public async Task GetMenuItemAsync_RelativeImageUrl_IsMadeAbsolute()
  {
    var service = CreateService(_ => JsonResponse(HttpStatusCode.OK,
      new MenuItemModel { Id = 7, Name = "Cake", ImageUrl = "images/cake.jpg" }));

    var item = await service.GetMenuItemAsync(7);

    Assert.NotNull(item);
    Assert.Equal("http://localhost:8000/images/cake.jpg", item!.ImageUrl);
  }

  [Fact]
  public async Task PostOrderAsync_Success_ReturnsConfirmation()
  {
    var service = CreateService(_ => JsonResponse(HttpStatusCode.OK,
      new OrderConfirmationDto
      {
        Id = 1,
        Status = "placed",
        TotalItems = 2,
        GrandTotal = 10.5,
        Message = "ok",
        EstimatedPrepMinutes = 15
      }));

    var result = await service.PostOrderAsync(new CreateOrderRequestDto());

    Assert.NotNull(result);
    Assert.Equal(1, result!.Id);
    Assert.Equal("placed", result.Status);
    Assert.Equal(15, result.EstimatedPrepMinutes);
  }

  [Theory]
  [InlineData(HttpStatusCode.BadRequest, 400, "There was a problem with your order.")]
  [InlineData(HttpStatusCode.NotFound, 404, "The requested item or order could not be found.")]
  [InlineData((HttpStatusCode)422, 422, "Some order details are invalid.")]
  public async Task PostOrderAsync_KnownHttpErrors_MapToExpectedApiException(HttpStatusCode statusCode, int expectedCode, string expectedMessage)
  {
    var service = CreateService(_ => StringResponse(statusCode, "details"));

    var ex = await Assert.ThrowsAsync<ApiException>(() => service.PostOrderAsync(new CreateOrderRequestDto()));

    Assert.Equal(expectedCode, ex.StatusCode);
    Assert.Contains(expectedMessage, ex.Message);
    Assert.Contains("details", ex.Message);
  }

  [Fact]
  public async Task PostOrderAsync_HttpRequestException_BecomesApiExceptionWithStatusZero()
  {
    var handler = new FakeHttpMessageHandler
    {
      Handler = (_, _) => throw new HttpRequestException("network down")
    };
    var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8000/") };
    var service = new ApiService(client);

    var ex = await Assert.ThrowsAsync<ApiException>(() => service.PostOrderAsync(new CreateOrderRequestDto()));

    Assert.Equal(0, ex.StatusCode);
    Assert.Contains("Network error:", ex.Message);
  }

  private static ApiService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
  {
    var handler = new FakeHttpMessageHandler
    {
      Handler = (request, _) => Task.FromResult(responseFactory(request))
    };
    var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8000/") };
    return new ApiService(client);
  }

  private static HttpResponseMessage JsonResponse<T>(HttpStatusCode statusCode, T payload)
  {
    return new HttpResponseMessage(statusCode)
    {
      Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    };
  }

  private static HttpResponseMessage StringResponse(HttpStatusCode statusCode, string body)
  {
    return new HttpResponseMessage(statusCode)
    {
      Content = new StringContent(body, Encoding.UTF8, "text/plain")
    };
  }
}
