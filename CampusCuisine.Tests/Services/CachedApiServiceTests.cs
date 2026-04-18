using System.Net.Http;
using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.Tests.TestDoubles;
using Xunit;

namespace CampusCuisine.Tests.Services;

public class CachedApiServiceTests
{
  [Fact]
  public async Task GetMenuByCategoryAsync_Success_PopulatesCacheAndReturnsItems()
  {
    var inner = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => Task.FromResult(new List<MenuItemModel>
      {
        new() { Id = 1, Name = "Grilled Chicken Burger" }
      })
    };
    var cache = new FakeMenuCache();
    var service = new CachedApiService(inner, cache);

    var result = await service.GetMenuByCategoryAsync("main");

    var single = Assert.Single(result);
    Assert.Equal("Grilled Chicken Burger", single.Name);
    Assert.True(cache.Store.ContainsKey("main"));
    Assert.Equal("Grilled Chicken Burger", cache.Store["main"].Single().Name);
  }

  [Fact]
  public async Task GetMenuByCategoryAsync_FailureWithCache_ReturnsCachedItems()
  {
    var inner = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => throw new HttpRequestException("network down")
    };
    var cache = new FakeMenuCache();
    cache.Save("main", new List<MenuItemModel>
    {
      new() { Id = 99, Name = "Cached Lasagne" }
    });
    var service = new CachedApiService(inner, cache);

    var result = await service.GetMenuByCategoryAsync("main");

    var single = Assert.Single(result);
    Assert.Equal("Cached Lasagne", single.Name);
  }

  [Fact]
  public async Task GetMenuByCategoryAsync_FailureWithNoCache_PropagatesException()
  {
    var inner = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => throw new HttpRequestException("network down")
    };
    var cache = new FakeMenuCache();
    var service = new CachedApiService(inner, cache);

    await Assert.ThrowsAsync<HttpRequestException>(() => service.GetMenuByCategoryAsync("main"));
  }

  [Fact]
  public async Task GetMenuByCategoryAsync_SuccessOverwritesPreviousCache()
  {
    var inner = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => Task.FromResult(new List<MenuItemModel>
      {
        new() { Id = 1, Name = "Fresh Burger" }
      })
    };
    var cache = new FakeMenuCache();
    cache.Save("main", new List<MenuItemModel>
    {
      new() { Id = 99, Name = "Stale Lasagne" }
    });
    var service = new CachedApiService(inner, cache);

    var result = await service.GetMenuByCategoryAsync("main");

    var single = Assert.Single(result);
    Assert.Equal("Fresh Burger", single.Name);
    Assert.Equal("Fresh Burger", cache.Store["main"].Single().Name);
  }

  [Fact]
  public async Task GetMenuItemAsync_PassesThroughUnchanged()
  {
    var inner = new FakeApiService
    {
      GetMenuItemAsyncHandler = id => Task.FromResult<MenuItemModel?>(new MenuItemModel { Id = id, Name = "Item" })
    };
    var cache = new FakeMenuCache();
    var service = new CachedApiService(inner, cache);

    var result = await service.GetMenuItemAsync(7);

    Assert.NotNull(result);
    Assert.Equal(7, result!.Id);
    Assert.Empty(cache.Store);
  }
}
