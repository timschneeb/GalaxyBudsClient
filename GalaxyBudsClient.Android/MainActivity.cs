using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;
using GalaxyBudsClient.Android.Impl;
using GalaxyBudsClient.Platform;
using ReactiveUI.Avalonia;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

#if NOT_DEBUGGABLE
[assembly: Application(Debuggable=true)]
#else
[assembly: Application(Debuggable=false)]
#endif

namespace GalaxyBudsClient.Android;

[Activity(
    Label = "Galaxy Buds Manager",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@mipmap/ic_launcher",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int RequestBluetoothPermission = 1;
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Program.Startup(false, new LogcatSink("GbClient"));
        PlatformImpl.InjectExternalBackend(new AndroidPlatformImplCreator(this));
        
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) {
#pragma warning disable CA1416
            ActivityCompat.RequestPermissions(this, [Manifest.Permission.BluetoothConnect], RequestBluetoothPermission);
#pragma warning restore CA1416
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            Toast.MakeText(this, "Location permission required to scan for Bluetooth devices nearby.", ToastLength.Long);
            ActivityCompat.RequestPermissions(this, [Manifest.Permission.AccessFineLocation], RequestBluetoothPermission);
        }
    }
    
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if (requestCode == RequestBluetoothPermission) 
        {
            // Check if the only required permission has been granted
            if (grantResults is not [Permission.Granted]) {    
                new AlertDialog.Builder(this)?
                    .SetTitle("Required permissions not granted")?
                    .SetMessage("Cannot start without the required Bluetooth permissions. Please visit the system settings to grant the missing permissions manually.")?
                    .SetNegativeButton("Close app", (_, _) => Finish())
                    .Show();
            }
        } 
        else 
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
