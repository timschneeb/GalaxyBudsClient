using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using Serilog;

namespace GalaxyBudsClient.Interface.MarkupExtensions;


public sealed class OpenLinkAction : AvaloniaObject, IAction
{
    public static readonly StyledProperty<string?> TargetUriProperty =
        AvaloniaProperty.Register<OpenLinkAction, string?>(nameof(TargetUri));
    
    public string? TargetUri
    {
        get => GetValue(TargetUriProperty);
        set => SetValue(TargetUriProperty, value);
    }

    public object Execute(object? sender, object? parameter)
    {
        if (TargetUri is null)
        {
            return false;
        }
        
        OpenLink(TargetUri);
        return true;
    }
    
    private static void OpenLink(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true, Verb = "open" });
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Cannot open link {Uri}", uri);
        }
    }
}