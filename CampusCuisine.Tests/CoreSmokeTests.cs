using CampusCuisine.Services;
using Xunit;

namespace CampusCuisine.Tests;

public class CoreSmokeTests
{
  [Fact]
  public void OrderState_StartsEmpty()
  {
    var state = new OrderState();

    Assert.False(state.HasOrder);
    Assert.Empty(state.Lines);
    Assert.Equal(0, state.TotalItems);
    Assert.Equal(0, state.GrandTotal);
  }
}
