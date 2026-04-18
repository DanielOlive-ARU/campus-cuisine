using System.Net.Http;

namespace CampusCuisine.Tests.TestDoubles;

public class FakeHttpMessageHandler : HttpMessageHandler
{
  public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? Handler { get; set; }

  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    if (Handler is null)
      throw new InvalidOperationException("Handler must be configured before use.");

    return Handler(request, cancellationToken);
  }
}
