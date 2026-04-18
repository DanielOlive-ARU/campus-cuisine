namespace CampusCuisine;

public partial class AppShell : Shell
{
  public AppShell()
  {
    InitializeComponent();

    // Decouple the closed-state flyout hamburger glyph (Shell.ForegroundColor)
    // and the page title text (Shell.TitleColor) from UserAppTheme.Light.
    // The rest of the app is pinned to Light so every MAUI-rendered page
    // is drawn on a forced-white background, but the title-bar region on
    // Windows is drawn by the OS chrome and still tracks the device
    // theme. On a dark-mode device that title-bar stays dark, so
    // black-on-dark hamburger and black-on-dark page title would both
    // be invisible.
    //
    // Read PlatformAppTheme (which reflects the device theme regardless
    // of our UserAppTheme override) and set both Shell colours to a
    // tone that contrasts with the native title-bar in that theme.
    // This is the only place in the app where device theme is allowed
    // to influence colour choice - everything else stays light.
    var deviceTheme = Application.Current?.PlatformAppTheme ?? AppTheme.Light;
    var titleBarForeground = deviceTheme == AppTheme.Dark
        ? Colors.White
        : Color.FromArgb("#1F2430");

    Shell.SetForegroundColor(this, titleBarForeground);
    Shell.SetTitleColor(this, titleBarForeground);
  }
}
