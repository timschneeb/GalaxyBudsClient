using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Android;

[Activity(
    Label = "Galaxy Buds Manager",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int RequestBluetoothPermission = 1;
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Program.Startup(false);
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
        else
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
                Toast.MakeText(this, "Required permissions not granted. Cannot start.", ToastLength.Long);
                Finish();
            }
        } 
        else 
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
