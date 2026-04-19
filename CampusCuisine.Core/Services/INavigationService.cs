namespace CampusCuisine.Services;

public interface INavigationService
{
  Task GoToAsync(string route);
}
