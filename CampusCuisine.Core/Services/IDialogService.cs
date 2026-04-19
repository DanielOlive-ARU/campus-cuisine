namespace CampusCuisine.Services;

public interface IDialogService
{
  Task ShowAsync(string title, string message, string ok);

  Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
