using System.Collections.ObjectModel;
using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class MenuItemCardSyncTests
{
  private static MenuItemModel Item(int id, string name = "X", decimal price = 1m) =>
    new() { Id = id, Name = name, Description = string.Empty, Price = price, ImageUrl = string.Empty };

  [Fact]
  public void Seed_NoItems_NoVms()
  {
    var items = new ObservableCollection<MenuItemModel>();
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();

    using var sync = new MenuItemCardSync(items, state, target);

    Assert.Empty(target);
  }

  [Fact]
  public void Seed_Items_NoCart_ProducesZeroQuantityVms()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1, "A"), Item(2, "B") };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();

    using var sync = new MenuItemCardSync(items, state, target);

    Assert.Equal(2, target.Count);
    Assert.Equal(1, target[0].Id);
    Assert.Equal("A", target[0].Name);
    Assert.Equal(0, target[0].Quantity);
    Assert.False(target[0].HasQuantity);
    Assert.Equal(2, target[1].Id);
    Assert.Equal(0, target[1].Quantity);
  }

  [Fact]
  public void Seed_Items_PreExistingCart_QuantitiesPopulated()
  {
    var state = new OrderState();
    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 4);
    state.AddLine(2, name: "B", unitPrice: 3.0, quantity: 1);

    var items = new ObservableCollection<MenuItemModel> { Item(1, "A"), Item(2, "B") };
    var target = new ObservableCollection<MenuItemCardViewModel>();

    using var sync = new MenuItemCardSync(items, state, target);

    Assert.Equal(4, target[0].Quantity);
    Assert.Equal(1, target[1].Quantity);
  }

  [Fact]
  public void Seed_CartIdNotInItems_IsIgnored()
  {
    var state = new OrderState();
    state.AddLine(99, name: "Orphan", unitPrice: 3.0, quantity: 2);

    var items = new ObservableCollection<MenuItemModel> { Item(1, "A") };
    var target = new ObservableCollection<MenuItemCardViewModel>();

    using var sync = new MenuItemCardSync(items, state, target);

    var vm = Assert.Single(target);
    Assert.Equal(1, vm.Id);
  }

  [Fact]
  public void Items_Add_AppendsNewVm()
  {
    var items = new ObservableCollection<MenuItemModel>();
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    items.Add(Item(5, "E"));

    var vm = Assert.Single(target);
    Assert.Equal(5, vm.Id);
    Assert.Equal("E", vm.Name);
    Assert.Equal(0, vm.Quantity);
  }

  [Fact]
  public void Items_AddWithExistingCart_VmSeedsFromCart()
  {
    var state = new OrderState();
    state.AddLine(5, name: "E", unitPrice: 3.0, quantity: 2);

    var items = new ObservableCollection<MenuItemModel>();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    items.Add(Item(5, "E"));

    var vm = Assert.Single(target);
    Assert.Equal(2, vm.Quantity);
  }

  [Fact]
  public void Items_Remove_RemovesMatchingVm()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1), Item(2) };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    items.RemoveAt(0);

    var vm = Assert.Single(target);
    Assert.Equal(2, vm.Id);
  }

  [Fact]
  public void Items_ClearAndReadd_RebuildsTarget()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1, "Old") };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    items.Clear();
    Assert.Empty(target);

    items.Add(Item(2, "New"));

    var vm = Assert.Single(target);
    Assert.Equal(2, vm.Id);
    Assert.Equal("New", vm.Name);
  }

  [Fact]
  public void Cart_AddLineForExistingVm_UpdatesQuantity()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1, "A", 5m) };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    var vm = target[0];

    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 2);

    Assert.Equal(2, vm.Quantity);
    Assert.True(vm.HasQuantity);
    Assert.Equal("In order: 2", vm.QuantityText);
  }

  [Fact]
  public void Cart_AddLineForUnknownId_DoesNothingToTarget()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1) };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    state.AddLine(99, name: "Orphan", unitPrice: 3.0, quantity: 1);

    Assert.Single(target);
    Assert.Equal(0, target[0].Quantity);
  }

  [Fact]
  public void Cart_RemoveLine_SetsVmQuantityToZero()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1) };
    var state = new OrderState();
    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 3);
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    Assert.Equal(3, target[0].Quantity);

    state.RemoveLine(1, 3);

    Assert.Equal(0, target[0].Quantity);
  }

  [Fact]
  public void Cart_SetQuantity_PreservesVmInstance()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1, "A", 5m) };
    var state = new OrderState();
    state.AddLine(1, name: "A", unitPrice: 5.0, quantity: 1);
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    var vmBefore = target[0];

    state.SetQuantity(1, 5);

    Assert.Same(vmBefore, target[0]);
    Assert.Equal(5, target[0].Quantity);
  }

  [Fact]
  public void Cart_Clear_ZeroesAllVmQuantities()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1), Item(2) };
    var state = new OrderState();
    state.AddLine(1, quantity: 2);
    state.AddLine(2, quantity: 3);
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    Assert.Equal(2, target[0].Quantity);
    Assert.Equal(3, target[1].Quantity);

    state.Clear();

    Assert.Equal(0, target[0].Quantity);
    Assert.Equal(0, target[1].Quantity);
  }

  [Fact]
  public void Cart_ClearThenAddNew_MirrorsCorrectly()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1) };
    var state = new OrderState();
    state.AddLine(1, quantity: 2);
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    state.Clear();
    state.AddLine(1, quantity: 4);

    Assert.Equal(4, target[0].Quantity);
  }

  [Fact]
  public void AddRemoveAddSameId_RebindsToNewEntry()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1) };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    using var sync = new MenuItemCardSync(items, state, target);

    state.AddLine(1, quantity: 2);
    Assert.Equal(2, target[0].Quantity);

    state.RemoveLine(1, 2);
    Assert.Equal(0, target[0].Quantity);

    state.AddLine(1, quantity: 5);
    Assert.Equal(5, target[0].Quantity);
  }

  [Fact]
  public void Dispose_StopsBothItemsAndCartPropagation()
  {
    var items = new ObservableCollection<MenuItemModel> { Item(1) };
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    var sync = new MenuItemCardSync(items, state, target);

    var vm = target[0];

    sync.Dispose();

    state.AddLine(1, quantity: 5);
    items.Add(Item(2));

    Assert.Single(target);
    Assert.Equal(0, vm.Quantity);
  }

  [Fact]
  public void Dispose_IsIdempotent()
  {
    var items = new ObservableCollection<MenuItemModel>();
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();
    var sync = new MenuItemCardSync(items, state, target);

    sync.Dispose();
    sync.Dispose();
  }

  [Fact]
  public void Ctor_NullArgs_Throw()
  {
    var items = new ObservableCollection<MenuItemModel>();
    var state = new OrderState();
    var target = new ObservableCollection<MenuItemCardViewModel>();

    Assert.Throws<ArgumentNullException>(() => new MenuItemCardSync(null!, state, target));
    Assert.Throws<ArgumentNullException>(() => new MenuItemCardSync(items, null!, target));
    Assert.Throws<ArgumentNullException>(() => new MenuItemCardSync(items, state, null!));
  }
}
