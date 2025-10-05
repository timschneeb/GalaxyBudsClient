using Avalonia;
using Avalonia.ReactiveUI;
using Foundation;
using UIKit;

namespace GalaxyBudsClient.iOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    private AppBuilder? _builder;

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        GalaxyBudsClient.Program.Startup(false);
        _builder = AppBuilder.Configure<GalaxyBudsClient.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI();
        _builder.StartWithClassicDesktopLifetime(Array.Empty<string>());
        return true;
    }
}

public class Program
{
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, nameof(AppDelegate));
    }
}
