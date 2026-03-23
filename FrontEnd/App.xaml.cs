namespace CampusCuisine;

using CampusCuisine;
using Microsoft.Maui.Controls;
using Microsoft.Maui.LifecycleEvents;
public partial class App : Application
{
  public App()
  {
    InitializeComponent();
  }
  protected override Window CreateWindow(IActivationState? activationState)
  {
    return new Window(new AppShell());
  }
}