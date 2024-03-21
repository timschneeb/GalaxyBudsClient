using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.InterfaceOld.Dialogs;

[Obsolete("Class will be removed during the redesign")]
public sealed class ManualPairDialog : Window
{
    private readonly IReadOnlyList<string> _modelCache
        = Enum.GetValues(typeof(Models)).Cast<Models>().Where(x => x != Models.NULL)
            .Select(x => x.GetModelMetadata()?.Name ?? string.Empty).ToList();

    public IEnumerable ModelSource => _modelCache;
    
    public readonly ComboBox Device;
    public readonly ComboBox Model;

    public Models SelectedModel { private set; get; } = Models.NULL;
    public string? SelectedDeviceMac { private set; get; }
        
    public ManualPairDialog()
    {
        AvaloniaXamlLoader.Load(this);
        this.AttachDevTools();

        DataContext = this;
            
        Device = this.GetControl<ComboBox>("Device");
        Model = this.GetControl<ComboBox>("Model");
            
        Init();
    }

    public new async Task<TResult> ShowDialog<TResult>(Window owner)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(300);
            return await base.ShowDialog<TResult>(owner);
        });
    }

    private async void Init()
    {
        Device.ItemsSource = await BluetoothService.Instance.GetDevicesAsync();
    }
        
    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void Apply_OnClick(object? sender, RoutedEventArgs e)
    {
        var modelIndex = Model.SelectedIndex;
        if (modelIndex < 0)
            SelectedModel = Models.NULL;
        else
            SelectedModel = (Models)modelIndex + 1;// +1 because NULL is not in list
        SelectedDeviceMac = (Device.SelectedItem as BluetoothDevice)?.Address;
        Close(true);
    }
}