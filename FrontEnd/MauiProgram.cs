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