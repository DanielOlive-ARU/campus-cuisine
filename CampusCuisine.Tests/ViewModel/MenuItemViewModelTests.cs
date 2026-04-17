using System.Net.Http;
using CampusCuisine.Models;
using CampusCuisine.Tests.TestDoubles;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class MenuItemViewModelTests
{
  [Theory]
  [InlineData("Starters", "appetizer")]
  [InlineData("Mains", "main")]
  [InlineData("Desserts", "dessert")]
  [InlineData("Drinks", "drinks")]
  public async Task InitializeAsync_MapsCategoryAndLoadsItems(string category, string expectedBackendCategory)
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = backendCategory => Task.FromResult(new List<MenuItemModel>
      {
        new() { Id = 1, Name = "Loaded" }
      })
    };
    var vm = new MenuItemViewModel(api, category);

    await vm.InitializeAsync();

    Assert.Equal(expectedBackendCategory, api.LastCategory);
    Assert.Single(vm.MenuItems);
    Assert.Equal("Loaded", vm.MenuItems[0].Name);
    Assert.False(vm.HasError);
    Assert.Equal(string.Empty, vm.ErrorMessage);
  }

  [Fact]
  public async Task InitializeAsync_Success_ClearsStaleItemsBeforeLoading()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => Task.FromResult(new List<MenuItemModel>
      {
        new() { Id = 2, Name = "Fresh" }
      })
    };
    var vm = new MenuItemViewModel(api, "Mains");
    vm.MenuItems.Add(new MenuItemModel { Id = 1, Name = "Stale" });

    await vm.InitializeAsync();

    Assert.Single(vm.MenuItems);
    Assert.Equal("Fresh", vm.MenuItems[0].Name);
  }

  [Fact]
  public async Task InitializeAsync_Success_ClearsOldErrorMessage()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => Task.FromResult(new List<MenuItemModel>())
    };
    var vm = new MenuItemViewModel(api, "Mains")
    {
      ErrorMessage = "Old error"
    };

    await vm.InitializeAsync();

    Assert.Equal(string.Empty, vm.ErrorMessage);
    Assert.False(vm.HasError);
  }

  [Fact]
  public async Task InitializeAsync_HttpRequestException_SetsUnavailableMessage()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => throw new HttpRequestException("down")
    };
    var vm = new MenuItemViewModel(api, "Mains");
    vm.MenuItems.Add(new MenuItemModel { Id = 1, Name = "Stale" });

    await vm.InitializeAsync();

    Assert.Empty(vm.MenuItems);
    Assert.Equal("The menu service is currently unavailable.", vm.ErrorMessage);
    Assert.True(vm.HasError);
  }

  [Fact]
  public async Task InitializeAsync_TaskCanceledException_SetsTimeoutMessage()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => throw new TaskCanceledException("timeout")
    };
    var vm = new MenuItemViewModel(api, "Mains");

    await vm.InitializeAsync();

    Assert.Empty(vm.MenuItems);
    Assert.Equal("The menu request timed out.", vm.ErrorMessage);
  }

  [Fact]
  public async Task InitializeAsync_GenericException_SetsFallbackMessage()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => throw new InvalidOperationException("boom")
    };
    var vm = new MenuItemViewModel(api, "Mains");

    await vm.InitializeAsync();

    Assert.Empty(vm.MenuItems);
    Assert.Equal("Unable to load menu items right now.", vm.ErrorMessage);
  }

  [Fact]
  public async Task InitializeAsync_IsBusy_PreventsReentry()
  {
    var gate = new TaskCompletionSource<List<MenuItemModel>>();
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => gate.Task
    };
    var vm = new MenuItemViewModel(api, "Mains");

    var firstCall = vm.InitializeAsync();
    var secondCall = vm.InitializeAsync();

    gate.SetResult(new List<MenuItemModel>
    {
      new() { Id = 1, Name = "Loaded once" }
    });

    await Task.WhenAll(firstCall, secondCall);

    Assert.Equal(1, api.GetMenuByCategoryAsyncCallCount);
    Assert.Single(vm.MenuItems);
  }
}
