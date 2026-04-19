using CampusCuisine.Models;
using Xunit;

namespace CampusCuisine.Tests.Models;

public class MenuItemSnapshotTests
{
  [Fact]
  public void Constructs_AndAccessesFields()
  {
    var snap = new MenuItemSnapshot("Burger", "Tasty", 8.5);

    Assert.Equal("Burger", snap.Name);
    Assert.Equal("Tasty", snap.Description);
    Assert.Equal(8.5, snap.UnitPrice);
  }

  [Fact]
  public void Record_EqualityByValue()
  {
    var a = new MenuItemSnapshot("Burger", "Tasty", 8.5);
    var b = new MenuItemSnapshot("Burger", "Tasty", 8.5);

    Assert.Equal(a, b);
    Assert.True(a == b);
    Assert.Equal(a.GetHashCode(), b.GetHashCode());
  }

  [Fact]
  public void Record_Inequality_DifferentName()
  {
    var a = new MenuItemSnapshot("Burger", "Tasty", 8.5);
    var b = new MenuItemSnapshot("Pizza", "Tasty", 8.5);

    Assert.NotEqual(a, b);
  }

  [Fact]
  public void Record_Inequality_DifferentDescription()
  {
    var a = new MenuItemSnapshot("Burger", "Tasty", 8.5);
    var b = new MenuItemSnapshot("Burger", "Stale", 8.5);

    Assert.NotEqual(a, b);
  }

  [Fact]
  public void Record_Inequality_DifferentPrice()
  {
    var a = new MenuItemSnapshot("Burger", "Tasty", 8.5);
    var b = new MenuItemSnapshot("Burger", "Tasty", 9.0);

    Assert.NotEqual(a, b);
  }
}
