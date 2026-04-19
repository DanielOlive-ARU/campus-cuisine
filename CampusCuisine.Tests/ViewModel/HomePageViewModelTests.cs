using System.Net.Http;
using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.Tests.TestDoubles;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class HomePageViewModelTests
{
  [Fact]
  public void Ctor_Defaults_CardsHiddenAndZeroTotals()
  {
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState());

    Assert.False(vm.FeaturedIsVisible);
    Assert.False(vm.IndulgenceIsVisible);
    Assert.Equal("0 items", vm.TotalItemsText);
    Assert.Equal("£0.00", vm.GrandTotalText);
    Assert.False(vm.HasOrder);
  }

  [Fact]
  public void Ctor_WithExistingOrderState_ReflectsTotals()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 4.5, quantity: 3);

    var vm = new HomePageViewModel(new FakeApiService(), state);

    Assert.Equal("3 items", vm.TotalItemsText);
    Assert.Equal("£13.50", vm.GrandTotalText);
    Assert.True(vm.HasOrder);
  }

  [Fact]
  public void Ctor_NullApi_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new HomePageViewModel(null!, new OrderState()));
  }

  [Fact]
  public void Ctor_NullOrderState_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new HomePageViewModel(new FakeApiService(), null!));
  }

  [Fact]
  public async Task InitializeAsync_LoadsFeaturedFromMains()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "main")
          return Task.FromResult(new List<MenuItemModel>
          {
            new() { Id = 1, Name = "Chicken Burger", Description = "Crispy", Price = 8.50m, ImageUrl = "/burger.jpg" }
          });
        return Task.FromResult(new List<MenuItemModel>());
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.True(vm.FeaturedIsVisible);
    Assert.Equal("Chicken Burger", vm.FeaturedName);
    Assert.Equal("Crispy", vm.FeaturedDescription);
    Assert.Equal("£8.50", vm.FeaturedPriceText);
    Assert.Equal("/burger.jpg", vm.FeaturedImageUrl);
  }

  [Fact]
  public async Task InitializeAsync_LoadsIndulgenceFromDesserts()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "dessert")
          return Task.FromResult(new List<MenuItemModel>
          {
            new() { Id = 10, Name = "Chocolate Cake", Description = "Rich", Price = 4.00m, ImageUrl = "/cake.jpg" }
          });
        return Task.FromResult(new List<MenuItemModel>());
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.True(vm.IndulgenceIsVisible);
    Assert.Equal("Chocolate Cake", vm.IndulgenceName);
    Assert.Equal("Rich", vm.IndulgenceDescription);
    Assert.Equal("£4.00", vm.IndulgencePriceText);
    Assert.Equal("/cake.jpg", vm.IndulgenceImageUrl);
  }

  [Fact]
  public async Task InitializeAsync_EmptyMains_KeepsFeaturedHidden()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = _ => Task.FromResult(new List<MenuItemModel>())
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.False(vm.FeaturedIsVisible);
    Assert.False(vm.IndulgenceIsVisible);
  }

  [Fact]
  public async Task InitializeAsync_EmptyMains_DoesNotAffectIndulgence()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "main")
          return Task.FromResult(new List<MenuItemModel>());
        return Task.FromResult(new List<MenuItemModel>
        {
          new() { Id = 1, Name = "Cake", Price = 2m }
        });
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.False(vm.FeaturedIsVisible);
    Assert.True(vm.IndulgenceIsVisible);
  }

  [Fact]
  public async Task InitializeAsync_MainsApiThrows_KeepsFeaturedHidden()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "main")
          throw new HttpRequestException("down");
        return Task.FromResult(new List<MenuItemModel>());
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.False(vm.FeaturedIsVisible);
  }

  [Fact]
  public async Task InitializeAsync_DessertsApiThrows_KeepsIndulgenceHidden()
  {
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "dessert")
          throw new HttpRequestException("down");
        return Task.FromResult(new List<MenuItemModel>());
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();

    Assert.False(vm.IndulgenceIsVisible);
  }

  [Fact]
  public async Task InitializeAsync_BothCategories_CachesAfterFirstLoad()
  {
    int mainCallCount = 0;
    int dessertCallCount = 0;
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "main") mainCallCount++;
        else if (category == "dessert") dessertCallCount++;
        return Task.FromResult(new List<MenuItemModel>
        {
          new() { Id = 1, Name = "X", Price = 1m }
        });
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();
    await vm.InitializeAsync();
    await vm.InitializeAsync();

    Assert.Equal(1, mainCallCount);
    Assert.Equal(1, dessertCallCount);
  }

  [Fact]
  public async Task InitializeAsync_AfterTransientFailure_RetriesOnNextCall()
  {
    int mainCallCount = 0;
    var api = new FakeApiService
    {
      GetMenuByCategoryAsyncHandler = category =>
      {
        if (category == "main")
        {
          mainCallCount++;
          if (mainCallCount == 1)
            throw new HttpRequestException("transient");
          return Task.FromResult(new List<MenuItemModel>
          {
            new() { Id = 1, Name = "Recovered", Price = 2m }
          });
        }
        return Task.FromResult(new List<MenuItemModel>());
      }
    };
    var vm = new HomePageViewModel(api, new OrderState());

    await vm.InitializeAsync();
    Assert.False(vm.FeaturedIsVisible);

    await vm.InitializeAsync();

    Assert.True(vm.FeaturedIsVisible);
    Assert.Equal("Recovered", vm.FeaturedName);
    Assert.Equal(2, mainCallCount);
  }

  [Fact]
  public void OrderStateAddLine_UpdatesTotals()
  {
    var state = new OrderState();
    var vm = new HomePageViewModel(new FakeApiService(), state);

    state.AddLine(1, unitPrice: 5.0, quantity: 2);

    Assert.Equal("2 items", vm.TotalItemsText);
    Assert.Equal("£10.00", vm.GrandTotalText);
    Assert.True(vm.HasOrder);
  }

  [Fact]
  public void OrderStateSingleItem_Singularises()
  {
    var state = new OrderState();
    var vm = new HomePageViewModel(new FakeApiService(), state);

    state.AddLine(1, quantity: 1);

    Assert.Equal("1 item", vm.TotalItemsText);
  }

  [Fact]
  public void OrderStateClear_ResetsTotals()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 3.0, quantity: 2);
    var vm = new HomePageViewModel(new FakeApiService(), state);

    state.Clear();

    Assert.Equal("0 items", vm.TotalItemsText);
    Assert.Equal("£0.00", vm.GrandTotalText);
    Assert.False(vm.HasOrder);
  }

  [Fact]
  public void Dispose_UnsubscribesFromOrderState()
  {
    var state = new OrderState();
    var vm = new HomePageViewModel(new FakeApiService(), state);

    vm.Dispose();

    state.AddLine(1, unitPrice: 10, quantity: 5);

    Assert.Equal("0 items", vm.TotalItemsText);
    Assert.Equal("£0.00", vm.GrandTotalText);
    Assert.False(vm.HasOrder);
  }

  [Fact]
  public void Dispose_IsIdempotent()
  {
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState());
    vm.Dispose();
    vm.Dispose();
  }

  [Fact]
  public async Task StartNewOrderCommand_EmptyCart_NavigatesWithoutDialog()
  {
    var nav = new FakeNavigationService();
    var dialog = new FakeDialogService();
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState(), dialog, nav);

    await ((AsyncRelayCommand)vm.StartNewOrderCommand).ExecuteAsync(null);

    Assert.Empty(dialog.ConfirmCalls);
    Assert.Contains("///StartersPage", nav.Routes);
  }

  [Fact]
  public async Task StartNewOrderCommand_ExistingCart_ConfirmAccept_ClearsAndNavigates()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 1);
    var nav = new FakeNavigationService();
    var dialog = new FakeDialogService { NextConfirmResponse = true };
    var vm = new HomePageViewModel(new FakeApiService(), state, dialog, nav);

    await ((AsyncRelayCommand)vm.StartNewOrderCommand).ExecuteAsync(null);

    Assert.Single(dialog.ConfirmCalls);
    Assert.False(state.HasOrder);
    Assert.Contains("///StartersPage", nav.Routes);
  }

  [Fact]
  public async Task StartNewOrderCommand_ExistingCart_ConfirmCancel_StaysPut()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 1);
    var nav = new FakeNavigationService();
    var dialog = new FakeDialogService { NextConfirmResponse = false };
    var vm = new HomePageViewModel(new FakeApiService(), state, dialog, nav);

    await ((AsyncRelayCommand)vm.StartNewOrderCommand).ExecuteAsync(null);

    Assert.True(state.HasOrder);
    Assert.Empty(nav.Routes);
  }

  [Fact]
  public async Task ContinueOrderCommand_NoOrder_DoesNotNavigate()
  {
    var nav = new FakeNavigationService();
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState(), dialogService: null, navigationService: nav);

    await ((AsyncRelayCommand)vm.ContinueOrderCommand).ExecuteAsync(null);

    Assert.Empty(nav.Routes);
  }

  [Fact]
  public async Task ContinueOrderCommand_HasOrder_NavigatesToSummary()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    var nav = new FakeNavigationService();
    var vm = new HomePageViewModel(new FakeApiService(), state, dialogService: null, navigationService: nav);

    await ((AsyncRelayCommand)vm.ContinueOrderCommand).ExecuteAsync(null);

    Assert.Contains("//OrderSummaryPage", nav.Routes);
  }

  [Fact]
  public async Task NavigateToCommand_WithRoute_Navigates()
  {
    var nav = new FakeNavigationService();
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState(), dialogService: null, navigationService: nav);

    await ((AsyncRelayCommand)vm.NavigateToCommand).ExecuteAsync("//MainsPage");

    Assert.Contains("//MainsPage", nav.Routes);
  }

  [Fact]
  public async Task NavigateToCommand_NullParameter_DoesNotNavigate()
  {
    var nav = new FakeNavigationService();
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState(), dialogService: null, navigationService: nav);

    await ((AsyncRelayCommand)vm.NavigateToCommand).ExecuteAsync(null);

    Assert.Empty(nav.Routes);
  }

  [Fact]
  public async Task NavigateToCommand_WhitespaceRoute_DoesNotNavigate()
  {
    var nav = new FakeNavigationService();
    var vm = new HomePageViewModel(new FakeApiService(), new OrderState(), dialogService: null, navigationService: nav);

    await ((AsyncRelayCommand)vm.NavigateToCommand).ExecuteAsync("   ");

    Assert.Empty(nav.Routes);
  }
}
