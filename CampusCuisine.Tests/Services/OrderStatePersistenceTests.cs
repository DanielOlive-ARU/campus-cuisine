using CampusCuisine.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CampusCuisine.Tests.Services;

/// <summary>
/// Pins down the shared-order-state contract that the frontend relies on across page navigations:
/// the concrete OrderState singleton and the IOrderStateService interface must resolve to the same
/// underlying instance, so state survives when a page re-resolves the service after navigation.
/// Mirrors the factory registration in FrontEnd/MauiProgram.cs.
/// </summary>
public class OrderStatePersistenceTests
{
  [Fact]
  public void IOrderStateService_And_OrderState_ResolveToSameInstance()
  {
    var provider = BuildServiceProvider();

    var concrete = provider.GetRequiredService<OrderState>();
    var viaInterface = provider.GetRequiredService<IOrderStateService>();

    Assert.Same(concrete, viaInterface);
  }

  [Fact]
  public void IOrderStateService_ResolvedTwice_ReturnsSameInstance()
  {
    var provider = BuildServiceProvider();

    var first = provider.GetRequiredService<IOrderStateService>();
    var second = provider.GetRequiredService<IOrderStateService>();

    Assert.Same(first, second);
  }

  [Fact]
  public void OrderStateMutation_IsVisibleOnNextResolve()
  {
    var provider = BuildServiceProvider();

    var firstResolve = provider.GetRequiredService<IOrderStateService>();
    firstResolve.AddLine(menuItemId: 42, name: "Burger", unitPrice: 8.5, quantity: 2);

    var secondResolve = provider.GetRequiredService<IOrderStateService>();

    Assert.True(secondResolve.HasOrder);
    Assert.Equal(2, secondResolve.TotalItems);
    Assert.Equal(17.0, secondResolve.GrandTotal);
    var line = Assert.Single(secondResolve.Lines);
    Assert.Equal(42, line.MenuItemId);
  }

  private static ServiceProvider BuildServiceProvider()
  {
    // Mirror FrontEnd/MauiProgram.cs: single underlying OrderState singleton; IOrderStateService
    // resolves to that same instance via factory. Any deviation (e.g. AddSingleton<IOrderStateService, OrderState>)
    // would split state and break these tests.
    var services = new ServiceCollection();
    services.AddSingleton<OrderState>();
    services.AddSingleton<IOrderStateService>(sp => sp.GetRequiredService<OrderState>());
    return services.BuildServiceProvider();
  }
}
