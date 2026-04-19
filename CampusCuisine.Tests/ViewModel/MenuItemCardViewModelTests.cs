using CampusCuisine.Models;
using CampusCuisine.Services;
using CampusCuisine.ViewModel;
using Xunit;

namespace CampusCuisine.Tests.ViewModel;

public class MenuItemCardViewModelTests
{
  [Fact]
  public void Ctor_Parameterless_DefaultsEverything()
  {
    var vm = new MenuItemCardViewModel();

    Assert.Equal(0, vm.Id);
    Assert.Equal(string.Empty, vm.Name);
    Assert.Equal(string.Empty, vm.Description);
    Assert.Equal(0m, vm.Price);
    Assert.Equal(string.Empty, vm.ImageUrl);
    Assert.Equal(0, vm.Quantity);
    Assert.False(vm.HasQuantity);
    Assert.Equal("In order: 0", vm.QuantityText);
  }

  [Fact]
  public void Ctor_FromMenuItemModel_CopiesFields()
  {
    var source = new MenuItemModel
    {
      Id = 7,
      Name = "Burger",
      Description = "Tasty",
      Price = 8.5m,
      ImageUrl = "/burger.jpg"
    };

    var vm = new MenuItemCardViewModel(source, quantity: 3);

    Assert.Equal(7, vm.Id);
    Assert.Equal("Burger", vm.Name);
    Assert.Equal("Tasty", vm.Description);
    Assert.Equal(8.5m, vm.Price);
    Assert.Equal("/burger.jpg", vm.ImageUrl);
    Assert.Equal(3, vm.Quantity);
  }

  [Fact]
  public void Ctor_FromMenuItemModel_DefaultsQuantityToZero()
  {
    var source = new MenuItemModel { Id = 1, Name = "X", Price = 1m };

    var vm = new MenuItemCardViewModel(source);

    Assert.Equal(0, vm.Quantity);
  }

  [Fact]
  public void Ctor_NullSource_Throws()
  {
    Assert.Throws<ArgumentNullException>(() => new MenuItemCardViewModel(null!, 0));
  }

  [Fact]
  public void Quantity_Setter_FiresPropertyChanged_ForQuantityHasQuantityAndQuantityText()
  {
    var vm = new MenuItemCardViewModel();
    var raised = new List<string?>();
    vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    vm.Quantity = 2;

    Assert.Contains(nameof(MenuItemCardViewModel.Quantity), raised);
    Assert.Contains(nameof(MenuItemCardViewModel.HasQuantity), raised);
    Assert.Contains(nameof(MenuItemCardViewModel.QuantityText), raised);
  }

  [Fact]
  public void Quantity_Setter_Idempotent_DoesNotFire()
  {
    var vm = new MenuItemCardViewModel();
    vm.Quantity = 3;
    var raised = new List<string?>();
    vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    vm.Quantity = 3;

    Assert.Empty(raised);
  }

  [Fact]
  public void HasQuantity_TrueWhenQuantityPositive()
  {
    var vm = new MenuItemCardViewModel { Quantity = 1 };
    Assert.True(vm.HasQuantity);
  }

  [Fact]
  public void HasQuantity_FalseWhenZero()
  {
    var vm = new MenuItemCardViewModel { Quantity = 0 };
    Assert.False(vm.HasQuantity);
  }

  [Fact]
  public void QuantityText_IncludesCount()
  {
    var vm = new MenuItemCardViewModel { Quantity = 5 };
    Assert.Equal("In order: 5", vm.QuantityText);
  }

  [Fact]
  public void Name_Setter_FiresPropertyChanged()
  {
    var vm = new MenuItemCardViewModel();
    var raised = new List<string?>();
    vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    vm.Name = "Burger";

    Assert.Contains(nameof(MenuItemCardViewModel.Name), raised);
  }

  [Fact]
  public void Price_Setter_FiresPropertyChanged()
  {
    var vm = new MenuItemCardViewModel();
    var raised = new List<string?>();
    vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

    vm.Price = 10.5m;

    Assert.Contains(nameof(MenuItemCardViewModel.Price), raised);
  }

  [Fact]
  public void AddCommand_WithOrderState_AddsLineWithSnapshot()
  {
    var state = new OrderState();
    var source = new MenuItemModel { Id = 7, Name = "Burger", Description = "Tasty", Price = 8.5m };
    var vm = new MenuItemCardViewModel(source, quantity: 0, orderState: state);

    vm.AddCommand.Execute(null);

    var line = Assert.Single(state.Lines);
    Assert.Equal(7, line.MenuItemId);
    Assert.Equal("Burger", line.Name);
    Assert.Equal("Tasty", line.Description);
    Assert.Equal(8.5, line.UnitPrice);
    Assert.Equal(1, line.Quantity);
  }

  [Fact]
  public void AddCommand_CalledTwice_AggregatesQuantity()
  {
    var state = new OrderState();
    var source = new MenuItemModel { Id = 1, Name = "A", Price = 2m };
    var vm = new MenuItemCardViewModel(source, quantity: 0, orderState: state);

    vm.AddCommand.Execute(null);
    vm.AddCommand.Execute(null);

    Assert.Equal(2, state.GetQuantityForMenuItem(1));
  }

  [Fact]
  public void DecreaseCommand_WithOrderState_DecrementsLine()
  {
    var state = new OrderState();
    state.AddLine(1, name: "A", unitPrice: 2.0, quantity: 3);
    var source = new MenuItemModel { Id = 1, Name = "A", Price = 2m };
    var vm = new MenuItemCardViewModel(source, quantity: 3, orderState: state);

    vm.DecreaseCommand.Execute(null);

    Assert.Equal(2, state.GetQuantityForMenuItem(1));
  }

  [Fact]
  public void AddCommand_WithoutOrderState_DoesNotThrow()
  {
    var source = new MenuItemModel { Id = 1, Name = "A", Price = 1m };
    var vm = new MenuItemCardViewModel(source);

    vm.AddCommand.Execute(null);
    vm.DecreaseCommand.Execute(null);
  }

  [Fact]
  public void Parameterless_Ctor_CommandsAreNoOp()
  {
    var vm = new MenuItemCardViewModel();

    vm.AddCommand.Execute(null);
    vm.DecreaseCommand.Execute(null);
  }
}
