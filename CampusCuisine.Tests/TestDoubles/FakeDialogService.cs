using CampusCuisine.Services;

namespace CampusCuisine.Tests.TestDoubles;

public class FakeDialogService : IDialogService
{
  public record ShowCall(string Title, string Message, string Ok);
  public record ConfirmCall(string Title, string Message, string Accept, string Cancel);

  public List<ShowCall> ShowCalls { get; } = new();
  public List<ConfirmCall> ConfirmCalls { get; } = new();

  public Func<ConfirmCall, bool>? ConfirmResponseFactory { get; set; }

  public bool NextConfirmResponse { get; set; } = true;

  public Task ShowAsync(string title, string message, string ok)
  {
    ShowCalls.Add(new ShowCall(title, message, ok));
    return Task.CompletedTask;
  }

  public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
  {
    var call = new ConfirmCall(title, message, accept, cancel);
    ConfirmCalls.Add(call);
    var result = ConfirmResponseFactory?.Invoke(call) ?? NextConfirmResponse;
    return Task.FromResult(result);
  }
}
