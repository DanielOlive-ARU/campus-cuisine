using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.Tests.TestDoubles;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class OrderSummaryLineViewModelTests
{
  [Fact]
  public void Quantity_Setter_NormalisesQuantityText()
  {
    var vm = new OrderSummaryLineViewModel();

    vm.Quantity = 7;

    Assert.Equal("7", vm.QuantityText);
  }

  [Fact]
  public void LineTotal_Recomputes_OnQuantityChange()
  {
    var vm = new OrderSummaryLineViewModel { UnitPrice = 4.5 };

    vm.Quantity = 3;

    Assert.Equal(13.5, vm.LineTotal);
  }

  [Fact]
  public void LineTotal_Recomputes_OnUnitPriceChange()
  {
    var vm = new OrderSummaryLineViewModel { Quantity = 2 };

    vm.UnitPrice = 6.25;

    Assert.Equal(12.5, vm.LineTotal);
  }

  [Fact]
  public void UpdateFrom_CopiesAllFields()
  {
    var source = new OrderLineEntry(11, new MenuItemSnapshot("Burger", "With cheese", 9.5), 2);
    var vm = new OrderSummaryLineViewModel();

    vm.UpdateFrom(source);

    Assert.Equal(11, vm.MenuItemId);
    Assert.Equal("Burger", vm.Name);
    Assert.Equal("With cheese", vm.Description);
    Assert.Equal(9.5, vm.UnitPrice);
    Assert.Equal(2, vm.Quantity);
    Assert.Equal("2", vm.QuantityText);
    Assert.Equal(19.0, vm.LineTotal);
  }

  [Fact]
  public void UpdateFrom_PreservesQuantityText_WhenQuantityUnchanged()
  {
    var vm = new OrderSummaryLineViewModel();
    vm.Quantity = 3;
    vm.QuantityText = "mid-edit";

    var source = new OrderLineEntry(1, new MenuItemSnapshot("X", "", 5), 3);

    vm.UpdateFrom(source);

    Assert.Equal("mid-edit", vm.QuantityText);
  }

  [Fact]
  public void Ctor_WithSource_SeedsAllFields()
  {
    var source = new OrderLineEntry(42, new MenuItemSnapshot("Pie", "Apple", 3.0), 4);

    var vm = new OrderSummaryLineViewModel(source);

    Assert.Equal(42, vm.MenuItemId);
    Assert.Equal("Pie", vm.Name);
    Assert.Equal("Apple", vm.Description);
    Assert.Equal(3.0, vm.UnitPrice);
    Assert.Equal(4, vm.Quantity);
    Assert.Equal("4", vm.QuantityText);
  }

  [Fact]
  public void TryValidateQuantity_AcceptsValidInteger()
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity("5", out var validated, out var err);

    Assert.True(ok);
    Assert.Equal(5, validated);
    Assert.Null(err);
  }

  [Fact]
  public void TryValidateQuantity_TrimsWhitespace()
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity("   5   ", out var validated, out var err);

    Assert.True(ok);
    Assert.Equal(5, validated);
    Assert.Null(err);
  }

  [Fact]
  public void TryValidateQuantity_Accepts999()
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity("999", out var validated, out _);

    Assert.True(ok);
    Assert.Equal(999, validated);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("abc")]
  [InlineData("5.0")]
  [InlineData("5e2")]
  [InlineData(null)]
  public void TryValidateQuantity_RejectsNonInteger(string? input)
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity(input, out var validated, out var err);

    Assert.False(ok);
    Assert.Equal(0, validated);
    Assert.Equal("Please enter a whole number quantity.", err);
  }

  [Theory]
  [InlineData("0")]
  [InlineData("-1")]
  [InlineData("-999")]
  public void TryValidateQuantity_RejectsNonPositive(string input)
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity(input, out var validated, out var err);

    Assert.False(ok);
    Assert.Equal(0, validated);
    Assert.Equal("Quantity must be greater than zero.", err);
  }

  [Theory]
  [InlineData("1000")]
  [InlineData("99999")]
  public void TryValidateQuantity_RejectsTooLarge(string input)
  {
    var ok = OrderSummaryLineViewModel.TryValidateQuantity(input, out var validated, out var err);

    Assert.False(ok);
    Assert.Equal(0, validated);
    Assert.Equal("Quantity is too large.", err);
  }

  [Fact]
  public void IncreaseCommand_WithOrderState_IncrementsQuantity()
  {
    var state = new OrderState();
    state.AddLine(1, name: "X", unitPrice: 2.0, quantity: 3);
    var entry = state.Lines[0];
    var vm = new OrderSummaryLineViewModel(entry, state, dialogService: null);

    vm.IncreaseCommand.Execute(null);

    Assert.Equal(4, state.GetQuantityForMenuItem(1));
  }

  [Fact]
  public void IncreaseCommand_WithoutOrderState_NoOp()
  {
    var vm = new OrderSummaryLineViewModel();
    vm.Quantity = 2;

    vm.IncreaseCommand.Execute(null);
  }

  [Fact]
  public async Task DecreaseCommand_QuantityGreaterThanOne_DecrementsWithoutDialog()
  {
    var state = new OrderState();
    state.AddLine(1, name: "X", unitPrice: 2.0, quantity: 5);
    var entry = state.Lines[0];
    var dialog = new FakeDialogService();
    var vm = new OrderSummaryLineViewModel(entry, state, dialog);

    await ((AsyncRelayCommand)vm.DecreaseCommand).ExecuteAsync(null);

    Assert.Equal(4, state.GetQuantityForMenuItem(1));
    Assert.Empty(dialog.ConfirmCalls);
  }

  [Fact]
  public async Task DecreaseCommand_QuantityOne_ConfirmAccept_RemovesLine()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 2.0, quantity: 1);
    var entry = state.Lines[0];
    var dialog = new FakeDialogService { NextConfirmResponse = true };
    var vm = new OrderSummaryLineViewModel(entry, state, dialog);

    await ((AsyncRelayCommand)vm.DecreaseCommand).ExecuteAsync(null);

    Assert.Equal(0, state.GetQuantityForMenuItem(1));
    var call = Assert.Single(dialog.ConfirmCalls);
    Assert.Equal("Remove Item", call.Title);
    Assert.Contains("Burger", call.Message);
  }

  [Fact]
  public async Task DecreaseCommand_QuantityOne_ConfirmCancel_KeepsLine()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 2.0, quantity: 1);
    var entry = state.Lines[0];
    var dialog = new FakeDialogService { NextConfirmResponse = false };
    var vm = new OrderSummaryLineViewModel(entry, state, dialog);

    await ((AsyncRelayCommand)vm.DecreaseCommand).ExecuteAsync(null);

    Assert.Equal(1, state.GetQuantityForMenuItem(1));
    Assert.Single(dialog.ConfirmCalls);
  }

  [Fact]
  public async Task DecreaseCommand_QuantityOne_NoDialogService_RemovesLine()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 2.0, quantity: 1);
    var entry = state.Lines[0];
    var vm = new OrderSummaryLineViewModel(entry, state, dialogService: null);

    await ((AsyncRelayCommand)vm.DecreaseCommand).ExecuteAsync(null);

    Assert.Equal(0, state.GetQuantityForMenuItem(1));
  }

  [Fact]
  public async Task RemoveCommand_ConfirmAccept_RemovesLineEntirely()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 2.0, quantity: 5);
    var entry = state.Lines[0];
    var dialog = new FakeDialogService { NextConfirmResponse = true };
    var vm = new OrderSummaryLineViewModel(entry, state, dialog);

    await ((AsyncRelayCommand)vm.RemoveCommand).ExecuteAsync(null);

    Assert.Equal(0, state.GetQuantityForMenuItem(1));
    Assert.Empty(state.Lines);
  }

  [Fact]
  public async Task RemoveCommand_ConfirmCancel_KeepsLine()
  {
    var state = new OrderState();
    state.AddLine(1, name: "Burger", unitPrice: 2.0, quantity: 5);
    var entry = state.Lines[0];
    var dialog = new FakeDialogService { NextConfirmResponse = false };
    var vm = new OrderSummaryLineViewModel(entry, state, dialog);

    await ((AsyncRelayCommand)vm.RemoveCommand).ExecuteAsync(null);

    Assert.Equal(5, state.GetQuantityForMenuItem(1));
  }

  [Fact]
  public async Task RemoveCommand_WithoutOrderState_NoOp()
  {
    var vm = new OrderSummaryLineViewModel();
    vm.Quantity = 3;

    await ((AsyncRelayCommand)vm.RemoveCommand).ExecuteAsync(null);
  }

  [Fact]
  public async Task RemoveCommand_WithoutDialog_RemovesLineImmediately()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 4);
    var entry = state.Lines[0];
    var vm = new OrderSummaryLineViewModel(entry, state, dialogService: null);

    await ((AsyncRelayCommand)vm.RemoveCommand).ExecuteAsync(null);

    Assert.Empty(state.Lines);
  }
}
