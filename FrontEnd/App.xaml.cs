using System;

namespace CampusCuisine
{
  public partial class App : Application
  {
    public static IServiceProvider Services { get; private set; } = default!;

    public App(IServiceProvider serviceProvider)
    {
      InitializeComponent();
      Services = serviceProvider;

      // Campus Cuisine is designed as a light-theme application. Pinning
      // the app theme here makes every AppThemeBinding across the MAUI
      // template's control styles (Shell flyout, Entry, Picker, Switch,
      // Slider, CheckBox, ActivityIndicator, Border, etc.) resolve to
      // their Light values, which keeps text readable on our brand
      // backgrounds regardless of the device's system theme. The
      // BrandPageBackground and BrandInk tokens applied to Page and the
      // default Label style act as explicit brand-consistent overrides
      // layered on top of this light-only policy.
      UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
      return new Window(new AppShell());
    }
  }
}