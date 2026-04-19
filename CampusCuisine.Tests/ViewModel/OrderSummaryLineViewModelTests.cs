using CampusCuisine.Models;
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
    var source = new OrderLineDto
    {
      MenuItemId = 11,
      Name = "Burger",
      Description = "With cheese",
      UnitPrice = 9.5,
      Quantity = 2
    };
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

    var source = new OrderLineDto
    {
      MenuItemId = 1,
      Name = "X",
      UnitPrice = 5,
      Quantity = 3
    };

    vm.UpdateFrom(source);

    Assert.Equal("mid-edit", vm.QuantityText);
  }

  [Fact]
  public void Ctor_WithSource_SeedsAllFields()
  {
    var source = new OrderLineDto
    {
      MenuItemId = 42,
      Name = "Pie",
      Description = "Apple",
      UnitPrice = 3.0,
      Quantity = 4
    };

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
}
