using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace GalaxyBudsClient.Interface.Developer;

public partial class TranslatorToolsView : UserControl
{
    public static bool GrantAllFeaturesForTesting { get; private set; }
    
    public TranslatorToolsView()
    {
        InitializeComponent();
        Loc.ErrorDetected += OnErrorDetected;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        Loc.ErrorDetected -= OnErrorDetected;
        base.OnUnloaded(e);
    }

    private void OnErrorDetected(string title, string content)
    {
        var td = new TaskDialog
        {
            Header = title,
            Buttons = { TaskDialogButton.CloseButton },
            IconSource = new SymbolIconSource { Symbol = Symbol.Warning },
            Content = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Text = content,
                MaxWidth = 450
            },
            XamlRoot = this
        };

        _ = td.ShowAsync();
    }

    private void OnGrantAllFeaturesChecked(object? sender, RoutedEventArgs e)
    {
        if(sender is not CheckBox control)
            return;
        
        GrantAllFeaturesForTesting = control.IsChecked ?? false;
        
        // Trigger RequiresFeatureBehavior update
        BluetoothImpl.Instance.Device.RaiseDeviceChanged();
    }
}