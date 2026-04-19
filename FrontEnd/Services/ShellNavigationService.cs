using CampusCuisine.Services;

namespace CampusCuisine.FrontEnd.Services;

public class ShellNavigationService : INavigationService
{
  public Task GoToAsync(string route)
  {
    if (Shell.Current is null)
      return Task.CompletedTask;

    return Shell.Current.GoToAsync(route);
  }
}
