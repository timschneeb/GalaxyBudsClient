using Android.Content;
using Android.Net;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Android.Impl;

public class DesktopServices(Context context) : BaseDesktopServices
{
    public override bool IsAutoStartEnabled { get; set; } = false;
    
    public override void OpenUri(string uri)
    {
        context.StartActivity(new Intent(Intent.ActionView, Uri.Parse(uri)));
    }
}