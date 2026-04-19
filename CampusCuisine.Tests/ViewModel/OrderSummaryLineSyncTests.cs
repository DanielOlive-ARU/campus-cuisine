using System.Collections.ObjectModel;
using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class OrderSummaryLineSyncTests
{
  [Fact]
  public void Seed_PreExistingLines_ProduceMatchingVMs()
  {
    var state = new OrderState();
    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 2);
    state.AddLine(2, name: "B", unitPrice: 3.0, quantity: 1);

    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    Assert.Equal(2, target.Count);
    Assert.Equal(1, target[0].MenuItemId);
    Assert.Equal("A", target[0].Name);
    Assert.Equal(2, target[0].Quantity);
    Assert.Equal(2, target[1].MenuItemId);
    Assert.Equal("B", target[1].Name);
  }

  [Fact]
  public void AddLine_AppendsNewVm()
  {
    var state = new OrderState();
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 1);

    var vm = Assert.Single(target);
    Assert.Equal(1, vm.MenuItemId);
    Assert.Equal("A", vm.Name);
    Assert.Equal(1, vm.Quantity);
  }

  [Fact]
  public void RemoveLine_RemovesMatchingVm()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    state.AddLine(2, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.RemoveLine(1, quantity: 1);

    var vm = Assert.Single(target);
    Assert.Equal(2, vm.MenuItemId);
  }

  [Fact]
  public void SetQuantity_UpdatesVmInPlace_SameInstance()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 4.0, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    var vmBefore = target[0];

    state.SetQuantity(1, 5);

    Assert.Same(vmBefore, target[0]);
    Assert.Equal(5, target[0].Quantity);
    Assert.Equal("5", target[0].QuantityText);
    Assert.Equal(20.0, target[0].LineTotal);
  }

  [Fact]
  public void AddRemoveAdd_SameMenuItemId_NoStaleSubscription()
  {
    var state = new OrderState();
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.AddLine(1, quantity: 1);
    var originalVm = target[0];

    state.RemoveLine(1, quantity: 1);
    Assert.Empty(target);

    var originalPostRemoveQuantity = originalVm.Quantity;

    state.AddLine(1, quantity: 1);
    var newVm = Assert.Single(target);

    Assert.NotSame(originalVm, newVm);

    state.SetQuantity(1, 7);

    Assert.Equal(7, newVm.Quantity);
    Assert.Equal(originalPostRemoveQuantity, originalVm.Quantity);
  }

  [Fact]
  public void Clear_EmptiesTarget()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    state.AddLine(2, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.Clear();

    Assert.Empty(target);
  }

  [Fact]
  public void Clear_UnsubscribesStaleVMs()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    var staleVm = target[0];

    state.Clear();

    state.AddLine(5, quantity: 1);
    state.SetQuantity(5, 3);

    Assert.Equal(1, staleVm.Quantity);
  }

  [Fact]
  public void Clear_ThenAddLine_WorksAfterReset()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.Clear();
    state.AddLine(2, name: "B", quantity: 3);

    var vm = Assert.Single(target);
    Assert.Equal(2, vm.MenuItemId);
    Assert.Equal(3, vm.Quantity);
  }

  [Fact]
  public void RapidAddLine_SameItem_AggregatesToSingleVm()
  {
    var state = new OrderState();
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.AddLine(1, unitPrice: 2.0, quantity: 2);
    state.AddLine(1, quantity: 3);

    var vm = Assert.Single(target);
    Assert.Equal(5, vm.Quantity);
    Assert.Equal(10.0, vm.LineTotal);
  }

  [Fact]
  public void DirectEntryQuantityMutation_MirrorsIntoVm()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    using var sync = new OrderSummaryLineSync(state, target);

    state.Lines[0].Quantity = 4;

    Assert.Equal(4, target[0].Quantity);
    Assert.Equal("4", target[0].QuantityText);
    Assert.Equal(8.0, target[0].LineTotal);
  }

  [Fact]
  public void Dispose_StopsPropagation()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 2.0, quantity: 1);
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    var sync = new OrderSummaryLineSync(state, target);
    var vm = target[0];

    sync.Dispose();

    state.AddLine(2, quantity: 1);
    state.SetQuantity(1, 9);

    Assert.Single(target);
    Assert.Equal(1, vm.Quantity);
  }

  [Fact]
  public void Dispose_IsIdempotent()
  {
    var state = new OrderState();
    var target = new ObservableCollection<OrderSummaryLineViewModel>();
    var sync = new OrderSummaryLineSync(state, target);

    sync.Dispose();
    sync.Dispose();
  }
}
