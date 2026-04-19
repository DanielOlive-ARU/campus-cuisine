using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.Tests.TestDoubles;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class OrderSummaryPageViewModelTests
{
  [Fact]
  public void Ctor_Defaults_EmptyLinesZeroTotals()
  {
    var vm = new OrderSummaryPageViewModel(new OrderState());

    Assert.Empty(vm.Lines);
    Assert.Equal("Total items: 0", vm.TotalItemsText);
    Assert.Equal("Grand total: £0.00", vm.GrandTotalText);
    Assert.False(vm.HasOrder);
  }

  [Fact]
  public void Ctor_NullOrderState_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new OrderSummaryPageViewModel(null!));
  }

  [Fact]
  public void Attach_SeedsLinesFromExistingOrderState()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 5.0, quantity: 2);
    state.AddLine(2, name: "Fries", unitPrice: 3.0, quantity: 1);

    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    Assert.Equal(2, vm.Lines.Count);
    Assert.Equal(1, vm.Lines[0].MenuItemId);
    Assert.Equal("Burger", vm.Lines[0].Name);
    Assert.Equal(2, vm.Lines[0].Quantity);
    Assert.Equal(2, vm.Lines[1].MenuItemId);
  }

  [Fact]
  public void Attach_BeforeMutations_ReflectsEmptyTotals()
  {
    var state = new OrderState();
    var vm = new OrderSummaryPageViewModel(state);

    vm.Attach();

    Assert.Equal("Total items: 0", vm.TotalItemsText);
    Assert.Equal("Grand total: £0.00", vm.GrandTotalText);
    Assert.False(vm.HasOrder);
  }

  [Fact]
  public void Attach_Idempotent_DoesNotDuplicateLines()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.0, quantity: 2);
    var vm = new OrderSummaryPageViewModel(state);

    vm.Attach();
    vm.Attach();
    vm.Attach();

    Assert.Single(vm.Lines);
    Assert.Equal(2, vm.Lines[0].Quantity);
  }

  [Fact]
  public void AddLine_AfterAttach_AppendsLine()
  {
    var state = new OrderState();
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    state.AddLine(1, name: "X", unitPrice: 5.0, quantity: 3);

    var line = Assert.Single(vm.Lines);
    Assert.Equal(1, line.MenuItemId);
    Assert.Equal(3, line.Quantity);
  }

  [Fact]
  public void SetQuantity_AfterAttach_UpdatesVmInPlace()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 4.0, quantity: 1);
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    var lineBefore = vm.Lines[0];

    state.SetQuantity(1, 5);

    Assert.Same(lineBefore, vm.Lines[0]);
    Assert.Equal(5, vm.Lines[0].Quantity);
    Assert.Equal("5", vm.Lines[0].QuantityText);
  }

  [Fact]
  public void Clear_AfterAttach_EmptiesLines()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 2);
    state.AddLine(2, quantity: 3);
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    state.Clear();

    Assert.Empty(vm.Lines);
  }

  [Fact]
  public void StateChange_RaisesPropertyChangedForTotals()
  {
    var state = new OrderState();
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    var raised = new List<string?>();
    vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    state.AddLine(1, unitPrice: 5.0, quantity: 2);

    Assert.Contains(nameof(OrderSummaryPageViewModel.TotalItemsText), raised);
    Assert.Contains(nameof(OrderSummaryPageViewModel.GrandTotalText), raised);
    Assert.Contains(nameof(OrderSummaryPageViewModel.HasOrder), raised);
  }

  [Fact]
  public void TotalsReflectCurrentOrderState()
  {
    var state = new OrderState();
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    state.AddLine(1, unitPrice: 4.5, quantity: 3);

    Assert.Equal("Total items: 3", vm.TotalItemsText);
    Assert.Equal("Grand total: £13.50", vm.GrandTotalText);
    Assert.True(vm.HasOrder);
  }

  [Fact]
  public void Detach_StopsPropagation()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.0, quantity: 2);
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    var linesBefore = vm.Lines.Count;

    vm.Detach();

    state.AddLine(2, unitPrice: 3.0, quantity: 1);
    state.SetQuantity(1, 7);

    // Lines collection is not maintained after detach.
    Assert.Equal(linesBefore, vm.Lines.Count);
    // Existing VM reference is not mirrored.
    Assert.Equal(2, vm.Lines[0].Quantity);
  }

  [Fact]
  public void DetachThenAttach_ReflectsCurrentState()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Old", unitPrice: 5.0, quantity: 2);
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    vm.Detach();

    state.AddLine(2, name: "New", unitPrice: 3.0, quantity: 4);

    vm.Attach();

    Assert.Equal(2, vm.Lines.Count);
    Assert.Contains(vm.Lines, l => l.MenuItemId == 1);
    Assert.Contains(vm.Lines, l => l.MenuItemId == 2 && l.Quantity == 4);
  }

  [Fact]
  public void Dispose_DetachesAndStopsPropagation()
  {
    var state = new OrderState();
    var vm = new OrderSummaryPageViewModel(state);
    vm.Attach();

    vm.Dispose();

    state.AddLine(1, unitPrice: 5.0, quantity: 2);

    Assert.Empty(vm.Lines);
  }

  [Fact]
  public void Dispose_IsIdempotent()
  {
    var vm = new OrderSummaryPageViewModel(new OrderState());
    vm.Attach();

    vm.Dispose();
    vm.Dispose();
  }

  [Fact]
  public void Attach_AfterDispose_Throws()
  {
    var vm = new OrderSummaryPageViewModel(new OrderState());
    vm.Dispose();

    Assert.Throws<ObjectDisposedException>(() => vm.Attach());
  }

  [Fact]
  public void PlaceOrderButtonText_DefaultsToPlaceOrder()
  {
    var vm = new OrderSummaryPageViewModel(new OrderState());

    Assert.Equal("Place Order", vm.PlaceOrderButtonText);
    Assert.False(vm.IsPlacingOrder);
  }

  [Fact]
  public async Task ClearOrderCommand_EmptyCart_DoesNothing()
  {
    var state = new OrderState();
    var dialog = new FakeDialogService();
    var vm = new OrderSummaryPageViewModel(state, api: null, dialogService: dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.ClearOrderCommand).ExecuteAsync(null);

    Assert.Empty(dialog.ConfirmCalls);
  }

  [Fact]
  public async Task ClearOrderCommand_WithOrder_ConfirmAccept_Clears()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 3);
    var dialog = new FakeDialogService { NextConfirmResponse = true };
    var vm = new OrderSummaryPageViewModel(state, api: null, dialogService: dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.ClearOrderCommand).ExecuteAsync(null);

    Assert.False(state.HasOrder);
    Assert.Single(dialog.ConfirmCalls);
  }

  [Fact]
  public async Task ClearOrderCommand_WithOrder_ConfirmCancel_KeepsOrder()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 3);
    var dialog = new FakeDialogService { NextConfirmResponse = false };
    var vm = new OrderSummaryPageViewModel(state, api: null, dialogService: dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.ClearOrderCommand).ExecuteAsync(null);

    Assert.True(state.HasOrder);
    Assert.Equal(3, state.TotalItems);
  }

  [Fact]
  public async Task PlaceOrderCommand_EmptyCart_ShowsAlertAndReturns()
  {
    var state = new OrderState();
    var dialog = new FakeDialogService();
    var api = new FakeApiService();
    var vm = new OrderSummaryPageViewModel(state, api, dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.PlaceOrderCommand).ExecuteAsync(null);

    var call = Assert.Single(dialog.ShowCalls);
    Assert.Equal("Order Empty", call.Title);
    Assert.Equal(0, api.GetMenuByCategoryAsyncCallCount);
  }

  [Fact]
  public async Task PlaceOrderCommand_HappyPath_PostsConfirmsClearsAndNavigates()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 5.0, quantity: 2);
    var dialog = new FakeDialogService();
    var nav = new FakeNavigationService();
    var api = new FakeApiService
    {
      PostOrderAsyncHandler = _ => Task.FromResult<OrderConfirmationDto?>(new OrderConfirmationDto
      {
        Id = 42,
        Status = "confirmed",
        TotalItems = 2,
        GrandTotal = 10.0,
        EstimatedPrepMinutes = 5
      })
    };
    var vm = new OrderSummaryPageViewModel(state, api, dialog, nav);

    await ((AsyncRelayCommand)vm.PlaceOrderCommand).ExecuteAsync(null);

    Assert.False(state.HasOrder);
    Assert.Contains("..", nav.Routes);
    var confirmCall = Assert.Single(dialog.ShowCalls);
    Assert.Equal("Order Confirmed", confirmCall.Title);
    Assert.Contains("Order ID: 42", confirmCall.Message);
  }

  [Fact]
  public async Task PlaceOrderCommand_ApiReturnsNull_ShowsOrderFailed()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.0, quantity: 1);
    var dialog = new FakeDialogService();
    var api = new FakeApiService
    {
      PostOrderAsyncHandler = _ => Task.FromResult<OrderConfirmationDto?>(null)
    };
    var vm = new OrderSummaryPageViewModel(state, api, dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.PlaceOrderCommand).ExecuteAsync(null);

    var call = Assert.Single(dialog.ShowCalls);
    Assert.Equal("Order Failed", call.Title);
    Assert.True(state.HasOrder);
  }

  [Fact]
  public async Task PlaceOrderCommand_ApiThrows_ShowsNetworkError()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.0, quantity: 1);
    var dialog = new FakeDialogService();
    var api = new FakeApiService
    {
      PostOrderAsyncHandler = _ => throw new HttpRequestException("boom")
    };
    var vm = new OrderSummaryPageViewModel(state, api, dialog, navigationService: null);

    await ((AsyncRelayCommand)vm.PlaceOrderCommand).ExecuteAsync(null);

    var call = Assert.Single(dialog.ShowCalls);
    Assert.Equal("Network Error", call.Title);
    Assert.True(state.HasOrder);
  }

  [Fact]
  public async Task PlaceOrderCommand_IsPlacingOrder_TogglesAroundWork()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.0, quantity: 1);
    var gate = new TaskCompletionSource<OrderConfirmationDto?>();
    var api = new FakeApiService
    {
      PostOrderAsyncHandler = _ => gate.Task
    };
    var vm = new OrderSummaryPageViewModel(state, api, dialogService: new FakeDialogService(), navigationService: new FakeNavigationService());

    var run = ((AsyncRelayCommand)vm.PlaceOrderCommand).ExecuteAsync(null);

    Assert.True(vm.IsPlacingOrder);
    Assert.Equal("Placing Order...", vm.PlaceOrderButtonText);

    gate.SetResult(new OrderConfirmationDto { Id = 1, Status = "confirmed", TotalItems = 1, GrandTotal = 5.0 });
    await run;

    Assert.False(vm.IsPlacingOrder);
    Assert.Equal("Place Order", vm.PlaceOrderButtonText);
  }
}
