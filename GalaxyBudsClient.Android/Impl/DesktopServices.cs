using Android.Content;
using Android.Net;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Android.Impl;

public class DesktopServices(Context context) : BaseDesktopServices
{
    public override bool IsAutoStartEnabled { get; set; } = false;
    
    public override void OpenUri(string uri)
    {
        var intent = new Intent(Intent.ActionView, Uri.Parse(uri));
        // The context is the Application, not an Activity, so a new task is required.
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }
}