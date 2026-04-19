using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CampusCuisine.FrontEnd.Services;
using CampusCuisine.Pages;
using CampusCuisine.Services;
using CampusCuisine.ViewModel;
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

      // MAUI-scoped abstractions over Shell.DisplayAlert and Shell.GoToAsync
      // let view-model commands raise dialogs and navigate without taking
      // a compile-time dependency on the MAUI framework types. Core stays
      // platform-neutral; the implementations live in the FrontEnd project.
      builder.Services.AddSingleton<IDialogService, MauiDialogService>();
      builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

      // Register page-scoped view-models so pages can receive them through
      // constructor injection rather than resolving services manually via
      // App.Services.GetRequiredService. MenuItemViewModel is not registered
      // here because its category is a runtime string, not a DI-resolvable
      // dependency; the category pages construct it explicitly.
      builder.Services.AddTransient<HomePageViewModel>();
      builder.Services.AddTransient<OrderSummaryPageViewModel>();

      // Register pages so Shell resolves them through the DI container; their
      // constructor dependencies (view-models or services) are injected by
      // the container rather than pulled from App.Services at runtime.
      builder.Services.AddTransient<HomePage>();
      builder.Services.AddTransient<MainsPage>();
      builder.Services.AddTransient<StartersPage>();
      builder.Services.AddTransient<DessertsPage>();
      builder.Services.AddTransient<OrderSummaryPage>();
      builder.Services.AddTransient<HelpPage>();

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
