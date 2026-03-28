using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CampusCuisine.Services;

namespace CampusCuisine
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

      // Single source of truth for API base URL.
      // Windows local: http://localhost:8000/
      // Android emulator (Google): http://10.0.2.2:8000/
      // Android emulator (Hyper-V): http://10.0.2.2:8000/
      // Physical device: http://192.168.0.125:8000/
      const string apiBaseUrl = "http://localhost:8000/";

      builder.Services.AddHttpClient<IApiService, ApiService>(client =>
      {
        client.BaseAddress = new Uri(apiBaseUrl);
      });

      builder.Services.AddSingleton<OrderState>();

#if DEBUG
      builder.Logging.AddDebug();
#endif

      return builder.Build();
    }
  }
}
