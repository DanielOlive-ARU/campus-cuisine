using CampusCuisine.Services;
using Xunit;

namespace CampusCuisine.Tests.Services;

public class OrderStateTests
{
  [Fact]
  public void AddLine_FirstItem_CreatesSingleLine()
  {
    var state = new OrderState();

    state.AddLine(1, name: "Burger", unitPrice: 8.5, quantity: 2, description: "Test");

    var line = Assert.Single(state.Lines);
    Assert.Equal(1, line.MenuItemId);
    Assert.Equal("Burger", line.Name);
    Assert.Equal("Test", line.Description);
    Assert.Equal(8.5, line.UnitPrice);
    Assert.Equal(2, line.Quantity);
  }

  [Fact]
  public void AddLine_SameItem_AggregatesQuantity()
  {
    var state = new OrderState();

    state.AddLine(1, name: "Burger", unitPrice: 8.5, quantity: 2);
    state.AddLine(1, quantity: 3);

    var line = Assert.Single(state.Lines);
    Assert.Equal(5, line.Quantity);
  }

  [Fact]
  public void AddLine_DifferentItems_CreatesMultipleLines()
  {
    var state = new OrderState();

    state.AddLine(1, quantity: 1);
    state.AddLine(2, quantity: 1);

    Assert.Equal(2, state.Lines.Count);
  }

  [Fact]
  public void RemoveLine_DecreasesQuantity_WhenLineRemains()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 3);

    state.RemoveLine(1, quantity: 1);

    var line = Assert.Single(state.Lines);
    Assert.Equal(2, line.Quantity);
  }

  [Fact]
  public void RemoveLine_RemovesLine_WhenQuantityFallsToZero()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);

    state.RemoveLine(1, quantity: 1);

    Assert.Empty(state.Lines);
    Assert.False(state.HasOrder);
  }

  [Fact]
  public void SetQuantity_UpdatesExistingLine()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);

    state.SetQuantity(1, 4);

    Assert.Equal(4, Assert.Single(state.Lines).Quantity);
  }

  [Fact]
  public void SetQuantity_Zero_RemovesExistingLine()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);

    state.SetQuantity(1, 0);

    Assert.Empty(state.Lines);
  }

  [Fact]
  public void Clear_RemovesAllLines()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);
    state.AddLine(2, quantity: 2);

    state.Clear();

    Assert.Empty(state.Lines);
    Assert.Equal(0, state.TotalItems);
    Assert.Equal(0, state.GrandTotal);
    Assert.False(state.HasOrder);
  }

  [Fact]
  public void TotalItems_ReturnsSumOfQuantities()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 2);
    state.AddLine(2, quantity: 3);

    Assert.Equal(5, state.TotalItems);
  }

  [Fact]
  public void GrandTotal_ReturnsSumOfLineTotals()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.5, quantity: 2);
    state.AddLine(2, unitPrice: 3.0, quantity: 3);

    Assert.Equal(20.0, state.GrandTotal);
  }

  [Fact]
  public void HasOrder_TracksPresenceOfLines()
  {
    var state = new OrderState();
    Assert.False(state.HasOrder);

    state.AddLine(1, quantity: 1);
    Assert.True(state.HasOrder);

    state.Clear();
    Assert.False(state.HasOrder);
  }

  [Fact]
  public void GetQuantityForMenuItem_ReturnsExistingQuantity()
  {
    var state = new OrderState();
    state.AddLine(10, quantity: 4);

    Assert.Equal(4, state.GetQuantityForMenuItem(10));
  }

  [Fact]
  public void GetQuantityForMenuItem_ReturnsZero_WhenItemMissing()
  {
    var state = new OrderState();

    Assert.Equal(0, state.GetQuantityForMenuItem(99));
  }

  [Fact]
  public void ToCreateOrderRequest_ContainsOnlyIdsAndQuantities()
  {
    var state = new OrderState();
    state.AddLine(7, name: "Burger", unitPrice: 9.5, quantity: 2, description: "Desc");

    var request = state.ToCreateOrderRequest();

    var line = Assert.Single(request.Items);
    Assert.Equal(7, line.MenuItemId);
    Assert.Equal(2, line.Quantity);
  }

  [Fact]
  public void RemoveLine_MissingItem_DoesNothing()
  {
    var state = new OrderState();
    state.AddLine(1, quantity: 1);

    state.RemoveLine(99, quantity: 1);

    Assert.Single(state.Lines);
    Assert.Equal(1, state.TotalItems);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void AddLine_NonPositiveQuantity_DoesNothing(int quantity)
  {
    var state = new OrderState();

    state.AddLine(1, quantity: quantity);

    Assert.Empty(state.Lines);
  }

  [Fact]
  public void AddLine_RaisesPropertyChanged_ForAggregateProperties()
  {
    var state = new OrderState();
    var raisedProperties = new List<string?>();
    state.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

    state.AddLine(1, unitPrice: 5.5, quantity: 2);

    Assert.Contains(nameof(IOrderStateService.Lines), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.TotalItems), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.GrandTotal), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.HasOrder), raisedProperties);
  }

  [Fact]
  public void SetQuantity_RaisesPropertyChanged_ForAggregateProperties()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.5, quantity: 1);
    var raisedProperties = new List<string?>();
    state.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

    state.SetQuantity(1, 4);

    Assert.Contains(nameof(IOrderStateService.Lines), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.TotalItems), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.GrandTotal), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.HasOrder), raisedProperties);
  }

  [Fact]
  public void MutatingExistingLineQuantity_RaisesPropertyChanged_ForAggregateProperties()
  {
    var state = new OrderState();
    state.AddLine(1, unitPrice: 5.5, quantity: 1);
    var line = Assert.Single(state.Lines);
    var raisedProperties = new List<string?>();
    state.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

    line.Quantity = 3;

    Assert.Contains(nameof(IOrderStateService.Lines), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.TotalItems), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.GrandTotal), raisedProperties);
    Assert.Contains(nameof(IOrderStateService.HasOrder), raisedProperties);
  }
}
