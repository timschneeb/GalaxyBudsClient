using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Interface.Services;

namespace GalaxyBudsClient.Interface.MarkupExtensions;


public class OpenLinkAction : AvaloniaObject, IAction
{
    public static readonly StyledProperty<string?> TargetUriProperty =
        AvaloniaProperty.Register<OpenLinkAction, string?>(nameof(TargetUri));
    
    public string? TargetUri
    {
        get => GetValue(TargetUriProperty);
        set => SetValue(TargetUriProperty, value);
    }

    public virtual object Execute(object? sender, object? parameter)
    {
        if (TargetUri is null)
        {
            return false;
        }
        
        _ = OpenLinkAsync(TargetUri);
        return true;
    }
    
    private static async Task OpenLinkAsync(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            await DialogHelper.ShowUnableToOpenLinkDialog(new Uri(uri));
        }
    }
}