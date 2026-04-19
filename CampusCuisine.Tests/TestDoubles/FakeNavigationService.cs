using CampusCuisine.Services;

namespace CampusCuisine.Tests.TestDoubles;

public class FakeNavigationService : INavigationService
{
  public List<string> Routes { get; } = new();

  public Task GoToAsync(string route)
  {
    Routes.Add(route);
    return Task.CompletedTask;
  }
}
