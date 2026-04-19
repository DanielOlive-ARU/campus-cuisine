using CampusCuisine.Models;
using Xunit;

namespace CampusCuisine.Tests.Models;

public class OrderLineEntryTests
{
  [Fact]
  public void Ctor_SetsFields()
  {
    var snap = new MenuItemSnapshot("Burger", "Tasty", 8.5);

    var entry = new OrderLineEntry(1, snap, 3);

    Assert.Equal(1, entry.MenuItemId);
    Assert.Same(snap, entry.Snapshot);
    Assert.Equal(3, entry.Quantity);
    Assert.Equal("Burger", entry.Name);
    Assert.Equal("Tasty", entry.Description);
    Assert.Equal(8.5, entry.UnitPrice);
    Assert.Equal(25.5, entry.LineTotal);
  }

  [Fact]
  public void Ctor_NullSnapshot_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new OrderLineEntry(1, null!, 1));
  }

  [Fact]
  public void Quantity_Setter_FiresPropertyChanged_ForQuantityAndLineTotal()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 1.0), 1);
    var raised = new List<string?>();
    entry.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    entry.Quantity = 5;

    Assert.Contains(nameof(OrderLineEntry.Quantity), raised);
    Assert.Contains(nameof(OrderLineEntry.LineTotal), raised);
  }

  [Fact]
  public void Quantity_Setter_Idempotent_DoesNotFire()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 1.0), 3);
    var raised = new List<string?>();
    entry.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    entry.Quantity = 3;

    Assert.Empty(raised);
  }

  [Fact]
  public void LineTotal_Computes_FromUnitPriceAndQuantity()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 2.5), 4);

    Assert.Equal(10.0, entry.LineTotal);
  }

  [Fact]
  public void LineTotal_Updates_WhenQuantityChanges()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 2.0), 2);

    entry.Quantity = 5;

    Assert.Equal(10.0, entry.LineTotal);
  }

  [Fact]
  public void Name_DelegatesTo_Snapshot()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("Burger", "", 1.0), 1);

    Assert.Equal("Burger", entry.Name);
  }

  [Fact]
  public void Description_DelegatesTo_Snapshot()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("", "Tasty", 1.0), 1);

    Assert.Equal("Tasty", entry.Description);
  }

  [Fact]
  public void UnitPrice_DelegatesTo_Snapshot()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("", "", 7.25), 1);

    Assert.Equal(7.25, entry.UnitPrice);
  }

  [Fact]
  public void Snapshot_Setter_UpdatesDerivedFields()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("Old", "OldDesc", 1.0), 2);

    entry.Snapshot = new MenuItemSnapshot("New", "NewDesc", 3.0);

    Assert.Equal("New", entry.Name);
    Assert.Equal("NewDesc", entry.Description);
    Assert.Equal(3.0, entry.UnitPrice);
    Assert.Equal(6.0, entry.LineTotal);
  }

  [Fact]
  public void Snapshot_Setter_FiresPropertyChanged_ForAllDerivedFields()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("Old", "OldDesc", 1.0), 2);
    var raised = new List<string?>();
    entry.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    entry.Snapshot = new MenuItemSnapshot("New", "NewDesc", 3.0);

    Assert.Contains(nameof(OrderLineEntry.Snapshot), raised);
    Assert.Contains(nameof(OrderLineEntry.Name), raised);
    Assert.Contains(nameof(OrderLineEntry.Description), raised);
    Assert.Contains(nameof(OrderLineEntry.UnitPrice), raised);
    Assert.Contains(nameof(OrderLineEntry.LineTotal), raised);
  }

  [Fact]
  public void Snapshot_Setter_IdempotentOnValueEquality_DoesNotFire()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 1.0), 2);
    var raised = new List<string?>();
    entry.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    entry.Snapshot = new MenuItemSnapshot("X", "Y", 1.0);

    Assert.Empty(raised);
  }

  [Fact]
  public void Snapshot_Setter_NullThrows()
  {
    var entry = new OrderLineEntry(1, new MenuItemSnapshot("X", "Y", 1.0), 1);

    Assert.Throws<ArgumentNullException>(() => entry.Snapshot = null!);
  }

  [Fact]
  public void MenuItemId_Immutable_SetAtConstruction()
  {
    var entry = new OrderLineEntry(42, new MenuItemSnapshot("X", "Y", 1.0), 1);

    Assert.Equal(42, entry.MenuItemId);
  }
}
