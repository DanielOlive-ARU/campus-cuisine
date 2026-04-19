using CampusCuisine.Services;

namespace CampusCuisine.FrontEnd.Services;

public class MauiDialogService : IDialogService
{
  public Task ShowAsync(string title, string message, string ok)
  {
    if (Shell.Current is null)
      return Task.CompletedTask;

    return Shell.Current.DisplayAlertAsync(title, message, ok);
  }

  public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
  {
    if (Shell.Current is null)
      return Task.FromResult(false);

    return Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
  }
}
