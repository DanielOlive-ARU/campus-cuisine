using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CampusCuisine.Services;
using Microsoft.Maui.Devices;

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

      var apiBaseUrl = GetApiBaseUrl();

      // Register the raw ApiService (HTTP client) as its concrete type so
      // the CachedApiService decorator can resolve it, then expose
      // IApiService as the decorated version. Every consumer transparently
      // receives the offline-fallback behaviour without knowing about it,
      // and the decorator stays platform-neutral (lives in
      // CampusCuisine.Core and is covered by unit tests).
      builder.Services.AddHttpClient<ApiService>(client =>
      {
        client.BaseAddress = new Uri(apiBaseUrl);
      });

      builder.Services.AddSingleton<IMenuCache, PreferencesMenuCache>();

      builder.Services.AddTransient<IApiService>(sp => new CachedApiService(
        sp.GetRequiredService<ApiService>(),
        sp.GetRequiredService<IMenuCache>()));

      builder.Services.AddSingleton<OrderState>();
      builder.Services.AddSingleton<IOrderStateService>(sp => sp.GetRequiredService<OrderState>());

#if DEBUG
      builder.Logging.AddDebug();
#endif

      return builder.Build();
    }

    private static string GetApiBaseUrl()
    {
      if (DeviceInfo.Platform == DevicePlatform.Android)
        return "http://10.0.2.2:8000/";

      if (DeviceInfo.Platform == DevicePlatform.WinUI)
        return "http://localhost:8000/";

      return "http://localhost:8000/";
    }
  }
}
