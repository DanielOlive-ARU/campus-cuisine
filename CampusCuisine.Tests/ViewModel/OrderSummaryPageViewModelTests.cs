using CampusCuisine.Services;
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
}
